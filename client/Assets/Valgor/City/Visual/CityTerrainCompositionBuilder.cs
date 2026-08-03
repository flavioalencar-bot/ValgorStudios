using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Monta a fundação natural (camadas 1–2) sob CityRoot.
    /// Idempotente: remove root anterior no reload.
    /// </summary>
    public static class CityTerrainCompositionBuilder
    {
        public static void Build(Transform cityRoot)
        {
            if (!CityCapitalWorldConfig.Enabled || cityRoot == null)
            {
                return;
            }

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

            // Slopes/embankments na periferia sul (acesso).
            CityTerrainAssetResolver.Spawn(
                "Embankment_A",
                props,
                new WorldPropPlacement("Embankment_A", 0.4f, 0.02f, -23.5f, 0f, 10f, 0.6f, 3f),
                false);
            CityTerrainAssetResolver.Spawn(
                "Slope_A",
                props,
                new WorldPropPlacement("Slope_A", -12f, 0.05f, -26f, 20f, 6f, 0.8f, 4f),
                false);

            if (quality >= CityAmbientQuality.Medium)
            {
                CityVegetationCompositionBuilder.Build(props);
            }

            ApplyFog(quality);
            Debug.Log(
                $"[Valgor.City] CapitalWorldFoundation montada (kit={CityCapitalWorldConfig.KitId}, " +
                $"quality={quality}, previewV2={CityCapitalLayoutRuntime.IsPreviewV2}).");
        }

        private static void ApplyFog(CityAmbientQuality quality)
        {
            if (!CityCapitalWorldConfig.AllowExtraFog(quality))
            {
                return;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = quality == CityAmbientQuality.High ? 0.012f : 0.008f;
            RenderSettings.fogColor = new Color(0.55f, 0.62f, 0.68f);
        }
    }
}
