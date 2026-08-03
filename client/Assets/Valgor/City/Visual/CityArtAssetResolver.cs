using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Resolve prefab artístico por BuildingId + qualidade, com fallback procedural.</summary>
    public static class CityArtAssetResolver
    {
        public static CityArtResolveResult ResolveBuilding(string buildingId, CityAmbientQuality quality)
        {
            var catalog = CityArtAssetCatalog.GetOrCreateRuntime();
            var entry = catalog.FindByBuildingId(buildingId);
            var rank = quality switch
            {
                CityAmbientQuality.High => 2,
                CityAmbientQuality.Medium => 1,
                _ => 0
            };

            var descriptor = entry == null
                ? null
                : new CityArtAssetDescriptor
                {
                    AssetId = entry.AssetId,
                    BuildingId = entry.BuildingId,
                    UsesExistingCastleLoader = entry.UsesExistingCastleLoader,
                    SlotReady = entry.SlotReady,
                    ContentAvailable = entry.IsAvailable
                };

            return CityArtAssetCatalogLogic.Resolve(
                descriptor,
                buildingId,
                highLoaded: entry?.HighFidelityPrefab != null,
                mediumLoaded: entry?.MediumPrefab != null,
                lowLoaded: entry?.LowPrefab != null,
                qualityRank: rank);
        }

        public static GameObject SelectPrefab(CityArtAssetEntry entry, CityArtResolveMode mode)
        {
            if (entry == null)
            {
                return null;
            }

            return mode switch
            {
                CityArtResolveMode.HighFidelity => entry.HighFidelityPrefab,
                CityArtResolveMode.Medium => entry.MediumPrefab != null ? entry.MediumPrefab : entry.HighFidelityPrefab,
                CityArtResolveMode.Low => entry.LowPrefab != null
                    ? entry.LowPrefab
                    : entry.MediumPrefab != null ? entry.MediumPrefab : entry.HighFidelityPrefab,
                _ => null
            };
        }
    }
}
