using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Água da fundação: canais/margens Tripo + plano de água Unity + espuma.
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
                var go = CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
                // Sheet só sobre mesh HF; fallback já traz canal + margem (evita faixa azul dupla).
                var isHf = go != null && !go.name.EndsWith("_Fallback");
                if (isHf && (p.AssetKey.StartsWith("River") || p.AssetKey is "Stream" or "Lake_A"))
                {
                    SpawnWaterSheet(root, p);
                }
            }

            foreach (var p in CityCapitalWorldConfig.WaterfallsAndBridges())
            {
                CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
            }

            if (CityCapitalWorldConfig.AllowWaterParticles(quality))
            {
                SpawnFoam(root, new Vector3(-28f, 0.08f, 17.2f), new Vector3(2.2f, 0.06f, 1.6f));
                SpawnFoam(root, new Vector3(28.5f, 0.08f, -11.2f), new Vector3(2.0f, 0.06f, 1.4f));
                SpawnFoam(root, new Vector3(-24.5f, 0.06f, 8f), new Vector3(1.6f, 0.05f, 1.2f));
            }
        }

        private static void SpawnWaterSheet(Transform parent, WorldPropPlacement channel)
        {
            var sheet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sheet.name = channel.AssetKey + "_WaterSheet";
            sheet.transform.SetParent(parent, false);
            sheet.transform.localPosition = new Vector3(channel.X, channel.Y + 0.06f, channel.Z);
            sheet.transform.localRotation = Quaternion.Euler(0f, channel.YawDegrees, 0f);
            sheet.transform.localScale = new Vector3(
                Mathf.Max(0.6f, channel.ScaleX * 0.72f),
                0.04f,
                Mathf.Max(0.6f, channel.ScaleZ * 0.72f));
            Object.Destroy(sheet.GetComponent<Collider>());
            var rend = sheet.GetComponent<Renderer>();
            rend.sharedMaterial = Valgor.Core.RuntimeSafeMaterials.Create(
                new Color(0.08f, 0.22f, 0.3f), "CapitalWaterSheet");
            if (CityAmbientQualitySettings.Current >= CityAmbientQuality.Medium)
            {
                sheet.AddComponent<CityWorldWaterScroll>();
            }
        }

        private static void SpawnFoam(Transform parent, Vector3 pos, Vector3 scale)
        {
            var foam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foam.name = "FoamHint";
            foam.transform.SetParent(parent, false);
            foam.transform.localPosition = pos;
            foam.transform.localScale = scale;
            Object.Destroy(foam.GetComponent<Collider>());
            CityVisualMaterials.Apply(foam.GetComponent<Renderer>(), new Color(0.82f, 0.9f, 0.95f, 0.75f));
        }
    }
}
