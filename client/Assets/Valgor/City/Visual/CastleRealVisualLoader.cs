using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Carrega o Castelo Tier 1 real (Resources) sob o filho Visual.
    /// Fallback: <see cref="CastleTierVisual"/> procedural — nunca como arte final.
    /// </summary>
    public static class CastleRealVisualLoader
    {
        public const string ResourcesKey = "Valgor/Castle_Tier1";
        public const string RealChildName = "Castle_Tier1_Real";

        /// <returns>true se o asset real foi anexado.</returns>
        public static bool TryAttach(Transform visualRoot, out string detail)
        {
            detail = "missing";
            if (visualRoot == null)
            {
                return false;
            }

            var prefab = Resources.Load<GameObject>(ResourcesKey);
            if (prefab == null)
            {
                detail = $"Resources.Load('{ResourcesKey}') == null";
                return false;
            }

            var instance = Object.Instantiate(prefab, visualRoot, false);
            instance.name = RealChildName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Visual only — seleção/collider ficam no BuildingSlot.
            foreach (var col in instance.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(col);
            }

            detail = $"ok prefab={prefab.name} renderers={instance.GetComponentsInChildren<Renderer>(true).Length}";
            Debug.Log($"[Valgor.City] Castle real visual attached: {detail}");
            return true;
        }
    }
}
