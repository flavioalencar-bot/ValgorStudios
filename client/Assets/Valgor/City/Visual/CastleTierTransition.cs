using System.Collections;
using UnityEngine;
using Valgor.City.Buildings;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Transição simples ao cruzar faixa de tier do Castelo:
    /// Tier atual → encolher → troca → crescer Tier seguinte.
    /// </summary>
    public sealed class CastleTierTransition : MonoBehaviour
    {
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
            var current = CastleRealVisualLoader.FindAttachedTier(root);
            Transform? oldChild = null;
            for (var i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i);
                if (c.name == CastleRealVisualLoader.RealChildNameForTier(current))
                {
                    oldChild = c;
                    break;
                }
            }

            const float shrink = 0.22f;
            var t0 = Time.unscaledTime;
            var startScale = oldChild != null ? oldChild.localScale : Vector3.one;
            while (oldChild != null && Time.unscaledTime - t0 < shrink)
            {
                var u = (Time.unscaledTime - t0) / shrink;
                oldChild.localScale = Vector3.Lerp(startScale, startScale * 0.82f, u);
                yield return null;
            }

            CastleRealVisualLoader.ClearCastleVisualChildren(root);
            if (!CastleRealVisualLoader.TryAttach(root, buildingLevel, out var detail))
            {
                Debug.LogWarning($"[Valgor.City] Castle tier transition failed: {detail}");
                CastleTierVisual.Build(root, Color.white, visualTier: 1);
            }
            else
            {
                var newTier = CastleRealVisualLoader.ResolveTier(buildingLevel);
                var newChild = root.Find(CastleRealVisualLoader.RealChildNameForTier(newTier));
                if (newChild == null && root.childCount > 0)
                {
                    newChild = root.GetChild(0);
                }

                if (newChild != null)
                {
                    newChild.localScale = Vector3.one * 0.88f;
                    const float grow = 0.28f;
                    t0 = Time.unscaledTime;
                    while (Time.unscaledTime - t0 < grow)
                    {
                        var u = (Time.unscaledTime - t0) / grow;
                        var eased = 1f - (1f - u) * (1f - u);
                        newChild.localScale = Vector3.Lerp(Vector3.one * 0.88f, Vector3.one, eased);
                        yield return null;
                    }

                    newChild.localScale = Vector3.one;
                }
            }

            var view = GetComponentInParent<BuildingView>();
            view?.RecacheAfterCastleVisualSwap();
            _running = null;
        }
    }
}
