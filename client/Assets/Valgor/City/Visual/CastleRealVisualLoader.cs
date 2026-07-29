using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Carrega o Castelo real (Resources/Valgor/Castle_TierN) sob o filho Visual,
    /// conforme a faixa de nível do edifício. Fallback: <see cref="CastleTierVisual"/>.
    /// </summary>
    public static class CastleRealVisualLoader
    {
        public const string RealChildPrefix = "Castle_Tier";
        public const string RealChildSuffix = "_Real";

        /// <summary>
        /// Faixas: 1–5→1 … 26–30→6.
        /// </summary>
        public static int ResolveTier(int buildingLevel)
        {
            var level = Math.Max(1, buildingLevel);
            if (level <= 5)
            {
                return 1;
            }

            if (level <= 10)
            {
                return 2;
            }

            if (level <= 15)
            {
                return 3;
            }

            if (level <= 20)
            {
                return 4;
            }

            if (level <= 25)
            {
                return 5;
            }

            return 6;
        }

        public static string ResourcesKeyForTier(int tier) => $"Valgor/Castle_Tier{Math.Clamp(tier, 1, 6)}";

        public static string RealChildNameForTier(int tier) =>
            $"{RealChildPrefix}{Math.Clamp(tier, 1, 6)}{RealChildSuffix}";

        /// <returns>true se algum asset real (tier pedido ou inferior) foi anexado.</returns>
        public static bool TryAttach(Transform visualRoot, int buildingLevel, out string detail)
        {
            detail = "missing";
            if (visualRoot == null)
            {
                return false;
            }

            ClearCastleVisualChildren(visualRoot);

            var want = ResolveTier(buildingLevel);
            for (var tier = want; tier >= 1; tier--)
            {
                if (TryAttachExactTier(visualRoot, tier, out detail))
                {
                    if (tier != want)
                    {
                        detail += $" (fallback from Tier{want})";
                        Debug.LogWarning($"[Valgor.City] Castle Tier{want} missing — using Tier{tier}.");
                    }

                    return true;
                }
            }

            detail = $"no real prefab for levels→Tier {want} (tried 1..{want})";
            return false;
        }

        /// <summary>Troca o visual se a faixa de tier mudou. Animação opcional ao cruzar faixa.</summary>
        /// <param name="deferred">true se a troca visual fica a cargo da transição (recache depois).</param>
        public static bool Sync(Transform visualRoot, int buildingLevel, bool animate, out string detail, out bool deferred)
        {
            detail = "noop";
            deferred = false;
            if (visualRoot == null)
            {
                detail = "null visual";
                return false;
            }

            var want = ResolveTier(buildingLevel);
            var current = FindAttachedTier(visualRoot);
            if (current == want)
            {
                detail = $"unchanged Tier{want}";
                return true;
            }

            if (current > 0)
            {
                Debug.Log(
                    $"Castle visual tier changed: T{current} -> T{want} at level {buildingLevel}");
            }

            if (animate && current > 0)
            {
                var host = visualRoot.GetComponent<CastleTierTransition>()
                    ?? visualRoot.gameObject.AddComponent<CastleTierTransition>();
                host.Play(want, buildingLevel);
                detail = $"transition Tier{current}→Tier{want}";
                deferred = true;
                return true;
            }

            return TryAttach(visualRoot, buildingLevel, out detail);
        }

        public static int FindAttachedTier(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return 0;
            }

            for (var i = 0; i < visualRoot.childCount; i++)
            {
                var child = visualRoot.GetChild(i);
                var name = child.name;
                if (!name.StartsWith(RealChildPrefix, StringComparison.Ordinal) ||
                    !name.EndsWith(RealChildSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                var mid = name.Substring(
                    RealChildPrefix.Length,
                    name.Length - RealChildPrefix.Length - RealChildSuffix.Length);
                if (int.TryParse(mid, out var tier) && tier >= 1 && tier <= 6)
                {
                    return tier;
                }
            }

            return 0;
        }

        public static bool IsRealCastleRenderer(Transform t)
        {
            for (var cur = t; cur != null; cur = cur.parent)
            {
                var n = cur.name;
                if (n.StartsWith(RealChildPrefix, StringComparison.Ordinal) &&
                    (n.EndsWith(RealChildSuffix, StringComparison.Ordinal) ||
                     n.EndsWith("_Visual", StringComparison.Ordinal)))
                {
                    return true;
                }

                if (string.Equals(n, "Visual", StringComparison.Ordinal) ||
                    n.StartsWith("Slot_", StringComparison.Ordinal))
                {
                    break;
                }
            }

            return false;
        }

        private static bool TryAttachExactTier(Transform visualRoot, int tier, out string detail)
        {
            var key = ResourcesKeyForTier(tier);
            var prefab = Resources.Load<GameObject>(key);
            if (prefab == null)
            {
                detail = $"Resources.Load('{key}') == null";
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, visualRoot, false);
            instance.name = RealChildNameForTier(tier);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            // Preserva escala do prefab (footprint progressivo no root do Visual).
            // NÃO forçar Vector3.one — isso esmagava o Tripo (~1 m) na City.

            foreach (var col in instance.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.Destroy(col);
            }

            detail =
                $"ok Tier{tier} prefab={prefab.name} " +
                $"renderers={instance.GetComponentsInChildren<Renderer>(true).Length}";
            Debug.Log($"[Valgor.City] Castle real visual attached: {detail}");
            return true;
        }

        internal static void ClearCastleVisualChildren(Transform visualRoot)
        {
            for (var i = visualRoot.childCount - 1; i >= 0; i--)
            {
                var child = visualRoot.GetChild(i);
                var n = child.name;
                var isReal = n.StartsWith(RealChildPrefix, StringComparison.Ordinal);
                var isProcedural = n.StartsWith("CastleTier", StringComparison.Ordinal);
                if (!isReal && !isProcedural)
                {
                    continue;
                }

                // Destroy é deferido — renomeia/desativa para Sync/Find não verem o antigo.
                child.name = "__CastleVisualPendingDestroy";
                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }
}
