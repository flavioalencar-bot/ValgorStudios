using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Monta a fundação natural (camadas 1–2) sob CityRoot.
    /// Em NoTripoCorrection: composição curada (sem vitrine de catálogo).
    /// </summary>
    public static class CityTerrainCompositionBuilder
    {
        public static void Build(Transform cityRoot)
        {
            if (!CityCapitalWorldConfig.Enabled || cityRoot == null)
            {
                return;
            }

            CityWorldPresentationConfig.ApplyFromCommandLine();

            if (CityCapitalWorldNoTripo.IsActive)
            {
                CityCapitalWorldCorrectionBuilder.Build(cityRoot);
                return;
            }

            // Caminho legado (preview foundation) — ainda disponível sem a flag NoTripo.
            BuildLegacyCatalogComposition(cityRoot);
        }

        private static void BuildLegacyCatalogComposition(Transform cityRoot)
        {
            var existing = cityRoot.Find(CityCapitalWorldConfig.WorldRootName);
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            var world = new GameObject(CityCapitalWorldConfig.WorldRootName).transform;
            world.SetParent(cityRoot, false);

            var terrain = new GameObject("TerrainModules").transform;
            terrain.SetParent(world, false);
            var water = new GameObject("WaterModules").transform;
            water.SetParent(world, false);
            var props = new GameObject("NatureProps").transform;
            props.SetParent(world, false);

            BuildOrganicGroundLegacy(terrain);
            CityHorizonBuilder.Build(terrain);

            foreach (var p in CityCapitalWorldConfig.NearCliffsAndPlateaus())
            {
                CityTerrainAssetResolver.Spawn(p.AssetKey, terrain, p, collidersEnabled: false);
            }

            CityWaterCompositionBuilder.Build(water);

            var quality = CityAmbientQualitySettings.Current;
            foreach (var p in CityCapitalWorldConfig.RockScatter(quality))
            {
                CityTerrainAssetResolver.Spawn(p.AssetKey, props, p, collidersEnabled: false);
            }

            ApplyFog(quality);
            Debug.Log(
                $"[Valgor.City] CapitalWorldFoundation legado (kit={CityCapitalWorldConfig.KitId}, " +
                $"quality={quality}). Preferir -capitalWorldNoTripoCorrection.");
        }

        private static void BuildOrganicGroundLegacy(Transform parent)
        {
            var root = new GameObject("OrganicGround").transform;
            root.SetParent(parent, false);
            SpawnGround(root, "OuterEarth", Vector3.zero, new Vector3(96f, 0.1f, 96f),
                new Color(0.28f, 0.32f, 0.22f), -0.1f);
            SpawnGround(root, "MidMeadow", Vector3.zero, new Vector3(62f, 0.09f, 62f),
                new Color(0.30f, 0.42f, 0.24f), -0.05f);
            SpawnGround(root, "CentralPlateau", new Vector3(0.4f, 0f, 1.2f), new Vector3(30f, 0.16f, 28f),
                new Color(0.38f, 0.34f, 0.28f), 0.03f);
        }

        private static void SpawnGround(
            Transform parent,
            string name,
            Vector3 pos,
            Vector3 scale,
            Color color,
            float y)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, y, pos.z);
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }

        private static void ApplyFog(CityAmbientQuality quality)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = quality switch
            {
                CityAmbientQuality.Low => 0.006f,
                CityAmbientQuality.High => 0.014f,
                _ => 0.01f
            };
            RenderSettings.fogColor = new Color(0.58f, 0.64f, 0.72f);
        }
    }
}
