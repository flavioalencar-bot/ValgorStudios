using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Dragons.Visual
{
    /// <summary>
    /// Asset opcional em Resources/Valgor/Dragons/DragonStageVisualCatalog para substituir placeholders.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DragonStageVisualCatalog",
        menuName = "Valgor/Dragons/Stage Visual Catalog")]
    public sealed class DragonStageVisualCatalogAsset : ScriptableObject
    {
        public List<DragonStageVisualConfig> Stages = new();
    }
}
