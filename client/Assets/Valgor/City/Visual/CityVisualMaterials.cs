using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Materiais URP/Built-in seguros para placeholders da cidade.
    /// </summary>
    public static class CityVisualMaterials
    {
        public static void Apply(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            var material = renderer.material;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            else
            {
                material.color = color;
            }
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
