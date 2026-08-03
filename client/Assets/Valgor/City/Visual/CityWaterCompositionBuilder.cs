using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Água da fundação: canais/margens/quedas do kit + material Unity (sem água animada do Tripo).
    /// </summary>
    public static class CityWaterCompositionBuilder
    {
        public static void Build(Transform parent)
        {
            var quality = CityAmbientQualitySettings.Current;
            var root = new GameObject("WorldWater").transform;
            root.SetParent(parent, false);

            foreach (var p in CityCapitalWorldConfig.RiverSegments())
            {
                CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
            }

            foreach (var p in CityCapitalWorldConfig.WaterfallsAndBridges())
            {
                CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
            }

            if (CityCapitalWorldConfig.AllowWaterParticles(quality) &&
                CityCapitalWorldConfig.WaterQualityFactor(quality) >= 0.8f)
            {
                SpawnFoam(root, new Vector3(-28f, 0.05f, 17.2f));
                SpawnFoam(root, new Vector3(28.5f, 0.05f, -11.2f));
            }
        }

        private static void SpawnFoam(Transform parent, Vector3 pos)
        {
            var foam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foam.name = "FoamHint";
            foam.transform.SetParent(parent, false);
            foam.transform.localPosition = pos;
            foam.transform.localScale = new Vector3(1.8f, 0.05f, 1.3f);
            Object.Destroy(foam.GetComponent<Collider>());
            CityVisualMaterials.Apply(foam.GetComponent<Renderer>(), new Color(0.75f, 0.85f, 0.9f, 0.7f));
        }
    }
}
