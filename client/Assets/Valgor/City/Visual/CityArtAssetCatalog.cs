using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valgor.City.Visual
{
    [Serializable]
    public sealed class CityArtAssetEntry
    {
        public string AssetId = string.Empty;
        public string BuildingId = string.Empty;
        public GameObject HighFidelityPrefab;
        public GameObject MediumPrefab;
        public GameObject LowPrefab;
        public bool UsesExistingCastleLoader;
        public bool SlotReady = true;

        public bool IsAvailable =>
            HighFidelityPrefab != null || MediumPrefab != null || LowPrefab != null || UsesExistingCastleLoader;
    }

    /// <summary>Catálogo central — preferir um asset SO; fallback in-memory se ausente.</summary>
    [CreateAssetMenu(fileName = "CityArtAssetCatalog", menuName = "Valgor/City/Art Asset Catalog")]
    public sealed class CityArtAssetCatalog : ScriptableObject
    {
        [SerializeField] private string packId = CityArtPipelineConfig.PackCoreId;
        [SerializeField] private List<CityArtAssetEntry> entries = new();

        public string PackId => packId;
        public IReadOnlyList<CityArtAssetEntry> Entries => entries;

        private static CityArtAssetCatalog _runtime;

        public static CityArtAssetCatalog GetOrCreateRuntime()
        {
            if (_runtime != null)
            {
                return _runtime;
            }

            var fromResources = Resources.Load<CityArtAssetCatalog>(CityArtPipelineConfig.CatalogResourcesKey);
            if (fromResources != null)
            {
                _runtime = fromResources;
                return _runtime;
            }

            _runtime = CreateInstance<CityArtAssetCatalog>();
            _runtime.packId = CityArtPipelineConfig.PackCoreId;
            _runtime.PopulateDefaultSlots();
            return _runtime;
        }

        public void PopulateDefaultSlots()
        {
            entries.Clear();
            var logic = CityArtAssetCatalogLogic.CreateDefaultBuildingSlots();
            for (var i = 0; i < logic.Count; i++)
            {
                var d = logic[i];
                entries.Add(new CityArtAssetEntry
                {
                    AssetId = d.AssetId,
                    BuildingId = d.BuildingId,
                    UsesExistingCastleLoader = d.UsesExistingCastleLoader,
                    SlotReady = d.SlotReady,
                    HighFidelityPrefab = null,
                    MediumPrefab = null,
                    LowPrefab = null
                });
            }
        }

        public CityArtAssetEntry FindByBuildingId(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId) || entries == null)
            {
                return null;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].BuildingId, buildingId, StringComparison.Ordinal))
                {
                    return entries[i];
                }
            }

            return null;
        }

        public int AvailableCount
        {
            get
            {
                var n = 0;
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].IsAvailable)
                    {
                        n++;
                    }
                }

                return n;
            }
        }
    }
}
