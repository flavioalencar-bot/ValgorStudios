using UnityEngine;

namespace Valgor.Heroes.Characters
{
    public sealed class HeroMaterialController : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        public void CaptureFromHierarchy()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void ApplySharedMaterials(Material[] materials)
        {
            if (renderers == null || materials == null || materials.Length == 0) return;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.sharedMaterials = materials;
            }
        }
    }
}
