using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Materiais seguros para placeholders (nunca magenta no URP).
    /// </summary>
    public static class CityVisualMaterials
    {
        public static void Apply(Renderer renderer, Color color)
        {
            RuntimeSafeMaterials.Apply(renderer, color);
        }

        public static Color MixState(Color identity, BuildingStateTint tint) => tint switch
        {
            BuildingStateTint.Ready => identity,
            BuildingStateTint.Available => Color.Lerp(identity, new Color(0.95f, 0.78f, 0.35f), 0.35f),
            BuildingStateTint.Locked => Color.Lerp(identity, new Color(0.22f, 0.24f, 0.28f), 0.72f),
            _ => identity
        };
    }

    public enum BuildingStateTint
    {
        Ready,
        Available,
        Locked
    }
}
