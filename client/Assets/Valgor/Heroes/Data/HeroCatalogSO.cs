using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Heroes.Data
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Hero Catalog", fileName = "HeroCatalog")]
    public sealed class HeroCatalogSO : ScriptableObject
    {
        public string Version = "1.0.0";
        public List<HeroDefinitionSO> Heroes = new();
    }
}
