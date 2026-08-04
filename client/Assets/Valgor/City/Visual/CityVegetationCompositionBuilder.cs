using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Vegetação distante — billboards (quads), sem árvores esféricas.
    /// Florestas densas HF ficam para etapa seguinte.
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

            var count = Mathf.RoundToInt(28 * density);
            for (var i = 0; i < count; i++)
            {
                var angle = i * (Mathf.PI * 2f / count) + 0.15f;
                var radius = CityCapitalWorldConfig.PlayableRadius + 8f + (i % 4) * 2.2f;
                var x = Mathf.Sin(angle) * radius;
                var z = Mathf.Cos(angle) * radius;
                if (CityCapitalWorldConfig.OverlapsReserved(x, z, 1.5f))
                {
                    continue;
                }

                // Billboard flat — silhueta de floresta, não esfera.
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                canopy.name = $"ForestBillboard_{i}";
                canopy.transform.SetParent(root, false);
                canopy.transform.localPosition = new Vector3(x, 1.4f + (i % 3) * 0.35f, z);
                canopy.transform.localScale = new Vector3(2.8f + (i % 3) * 0.6f, 2.2f + (i % 4) * 0.4f, 0.35f);
                canopy.transform.LookAt(new Vector3(0f, canopy.transform.position.y, 0f));
                Object.Destroy(canopy.GetComponent<Collider>());
                CityVisualMaterials.Apply(
                    canopy.GetComponent<Renderer>(),
                    Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.12f, 0.22f, 0.1f), 0.45f));
            }
        }
    }
}
