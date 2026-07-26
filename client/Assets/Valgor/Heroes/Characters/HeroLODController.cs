using UnityEngine;

namespace Valgor.Heroes.Characters
{
    public sealed class HeroLODController : MonoBehaviour
    {
        [SerializeField] private LODGroup lodGroup;

        public LODGroup LodGroup => lodGroup;

        public void Bind(LODGroup group) => lodGroup = group;

        public void EnsureGroup()
        {
            if (lodGroup == null)
                lodGroup = GetComponent<LODGroup>() ?? gameObject.AddComponent<LODGroup>();
        }
    }
}
