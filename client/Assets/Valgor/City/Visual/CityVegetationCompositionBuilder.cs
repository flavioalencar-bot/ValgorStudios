using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Vegetação distante/periférica da fundação (sem atravessar slots).
    /// Florestas densas completas ficam para etapa seguinte.
    /// </summary>
    public static class CityVegetationCompositionBuilder
    {
        public static void Build(Transform parent)
        {
            var quality = CityAmbientQualitySettings.Current;
            var density = CityCapitalWorldConfig.VegetationDensity(quality);
            if (density < 0.2f)
            {
                return;
            }

            var root = new GameObject("WorldVegetationHints").transform;
            root.SetParent(parent, false);

            var count = Mathf.RoundToInt(18 * density);
            for (var i = 0; i < count; i++)
            {
                var angle = i * (Mathf.PI * 2f / count);
                var radius = CityCapitalWorldConfig.PlayableRadius + 6f + (i % 3) * 2.5f;
                var x = Mathf.Sin(angle) * radius;
                var z = Mathf.Cos(angle) * radius;
                if (CityCapitalWorldConfig.OverlapsReserved(x, z, 1.2f))
                {
                    continue;
                }

                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.name = $"TreeHint_{i}";
                tree.transform.SetParent(root, false);
                tree.transform.localPosition = new Vector3(x, 0.9f, z);
                tree.transform.localScale = new Vector3(0.55f, 1.1f + (i % 3) * 0.15f, 0.55f);
                Object.Destroy(tree.GetComponent<Collider>());
                CityVisualMaterials.Apply(tree.GetComponent<Renderer>(), CityVisualMaterials.Vegetation);

                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.name = "Canopy";
                canopy.transform.SetParent(tree.transform, false);
                canopy.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                canopy.transform.localScale = new Vector3(2.2f, 1.4f, 2.2f);
                Object.Destroy(canopy.GetComponent<Collider>());
                CityVisualMaterials.Apply(
                    canopy.GetComponent<Renderer>(),
                    Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.2f, 0.35f, 0.18f), 0.35f));
            }
        }
    }
}
