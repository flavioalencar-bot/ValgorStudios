using System.Collections;
using UnityEngine;
using Valgor.City.Buildings;
using Valgor.City.Camera;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Troca suave do filho visual do Castelo (fade) sem mover câmera nem o root lógico.
    /// </summary>
    public sealed class CastleTierTransition : MonoBehaviour
    {
        private const float FadeOutSeconds = 0.18f;
        private const float FadeInSeconds = 0.22f;

        private Coroutine? _running;

        public void Play(int targetTier, int buildingLevel)
        {
            if (_running != null)
            {
                StopCoroutine(_running);
            }

            _running = StartCoroutine(Run(buildingLevel));
        }

        private IEnumerator Run(int buildingLevel)
        {
            var root = transform;
            var logicalRoot = GetComponentInParent<BuildingView>()?.transform;
            var logicalPos = logicalRoot != null ? logicalRoot.position : Vector3.zero;
            var logicalRot = logicalRoot != null ? logicalRoot.rotation : Quaternion.identity;
            var logicalScale = logicalRoot != null ? logicalRoot.localScale : Vector3.one;

            var cam = FindFirstObjectByType<CityCameraController>();
            cam?.LockPose();
            cam?.SuppressFocus(1.0f);

            var oldChild = FindActiveRealChild(root);

            if (!CastleRealVisualLoader.TryAttachExactTier(
                    root,
                    CastleRealVisualLoader.ResolveTier(buildingLevel),
                    out var detail))
            {
                Debug.LogWarning($"[Valgor.City] Castle tier transition failed: {detail}");
                CastleRealVisualLoader.ClearCastleVisualChildren(root);
                CastleTierVisual.Build(root, Color.white, visualTier: 1);
                Finish(cam, logicalRoot, logicalPos, logicalRot, logicalScale);
                yield break;
            }

            var newTier = CastleRealVisualLoader.ResolveTier(buildingLevel);
            var newChild = root.Find(CastleRealVisualLoader.RealChildNameForTier(newTier));
            if (newChild == null)
            {
                for (var i = 0; i < root.childCount; i++)
                {
                    var c = root.GetChild(i);
                    if (c != oldChild &&
                        c.name.StartsWith(CastleRealVisualLoader.RealChildPrefix, System.StringComparison.Ordinal))
                    {
                        newChild = c;
                        break;
                    }
                }
            }

            if (newChild == null)
            {
                Finish(cam, logicalRoot, logicalPos, logicalRot, logicalScale);
                yield break;
            }

            var newTargetScale = newChild.localScale;
            SetRenderersFade(newChild, 0f, 1f);
            newChild.gameObject.SetActive(true);

            var t0 = Time.unscaledTime;
            while (oldChild != null && Time.unscaledTime - t0 < FadeOutSeconds)
            {
                var u = (Time.unscaledTime - t0) / FadeOutSeconds;
                SetRenderersFade(oldChild, 1f - u, 1f);
                RestoreLogical(logicalRoot, logicalPos, logicalRot, logicalScale);
                yield return null;
            }

            if (oldChild != null)
            {
                oldChild.name = "__CastleVisualPendingDestroy";
                oldChild.gameObject.SetActive(false);
                Destroy(oldChild.gameObject);
            }

            newChild.localScale = newTargetScale;
            t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < FadeInSeconds)
            {
                var u = (Time.unscaledTime - t0) / FadeInSeconds;
                var eased = 1f - (1f - u) * (1f - u);
                var brightness = 1f + 0.12f * Mathf.Sin(eased * Mathf.PI);
                SetRenderersFade(newChild, eased, brightness);
                RestoreLogical(logicalRoot, logicalPos, logicalRot, logicalScale);
                yield return null;
            }

            SetRenderersFade(newChild, 1f, 1f);
            newChild.localScale = newTargetScale;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var c = root.GetChild(i);
                if (c == newChild)
                {
                    continue;
                }

                var n = c.name;
                if (n.StartsWith(CastleRealVisualLoader.RealChildPrefix, System.StringComparison.Ordinal) ||
                    n.StartsWith("CastleTier", System.StringComparison.Ordinal) ||
                    n.StartsWith("__CastleVisual", System.StringComparison.Ordinal))
                {
                    c.gameObject.SetActive(false);
                    Destroy(c.gameObject);
                }
            }

            Finish(cam, logicalRoot, logicalPos, logicalRot, logicalScale);
        }

        private void Finish(
            CityCameraController? cam,
            Transform? logicalRoot,
            Vector3 logicalPos,
            Quaternion logicalRot,
            Vector3 logicalScale)
        {
            RestoreLogical(logicalRoot, logicalPos, logicalRot, logicalScale);
            cam?.UnlockPose();
            cam?.SuppressFocus(0.35f);

            var view = GetComponentInParent<BuildingView>();
            view?.RecacheAfterCastleVisualSwap();
            _running = null;
        }

        private static void RestoreLogical(
            Transform? logicalRoot,
            Vector3 pos,
            Quaternion rot,
            Vector3 scale)
        {
            if (logicalRoot == null)
            {
                return;
            }

            logicalRoot.SetPositionAndRotation(pos, rot);
            logicalRoot.localScale = scale;
        }

        private static Transform? FindActiveRealChild(Transform root)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i);
                if (!c.gameObject.activeSelf)
                {
                    continue;
                }

                var n = c.name;
                if (n.StartsWith(CastleRealVisualLoader.RealChildPrefix, System.StringComparison.Ordinal) &&
                    n.EndsWith(CastleRealVisualLoader.RealChildSuffix, System.StringComparison.Ordinal))
                {
                    return c;
                }
            }

            return null;
        }

        private static void SetRenderersFade(Transform root, float alpha, float brightness)
        {
            alpha = Mathf.Clamp01(alpha);
            brightness = Mathf.Clamp(brightness, 0.5f, 1.6f);
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null)
                {
                    continue;
                }

                var block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                WriteColor(block, r.sharedMaterial, "_BaseColor", alpha, brightness);
                WriteColor(block, r.sharedMaterial, "_Color", alpha, brightness);
                r.SetPropertyBlock(block);
                r.enabled = alpha > 0.04f;
            }
        }

        private static void WriteColor(
            MaterialPropertyBlock block,
            Material? shared,
            string prop,
            float alpha,
            float brightness)
        {
            if (shared == null || !shared.HasProperty(prop))
            {
                return;
            }

            var c = shared.GetColor(prop);
            c.r *= brightness;
            c.g *= brightness;
            c.b *= brightness;
            c.a = alpha;
            block.SetColor(prop, c);
        }
    }
}
