using System;
using System.Collections.Generic;
using System.Globalization;
using Valgor.City.Data;

namespace Valgor.City.Production
{
    public interface IProductionRepository
    {
        ProductionSnapshot? Load();
        void Save(ProductionSnapshot snapshot);
    }

    public sealed class ProductionSnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public Dictionary<string, BuildingProductionState> Buildings { get; set; } = new();
        public Dictionary<ResourceType, long> Wallet { get; set; } = new();
    }

    /// <summary>
    /// Persistência local técnica. Em memória cobre City↔WorldMap; PlayerPrefs cobre restart.
    /// Contrato preparado para backend (Load/Save de snapshot).
    /// </summary>
    public sealed class LocalProductionRepository : IProductionRepository
    {
        private readonly string _keyPrefix;
        private ProductionSnapshot? _memory;

        public LocalProductionRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix;
        }

        public ProductionSnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

            var loaded = LoadFromPrefs();
            if (loaded != null)
            {
                _memory = Clone(loaded);
            }

            return loaded;
        }

        public void Save(ProductionSnapshot snapshot)
        {
            _memory = Clone(snapshot);
            SaveToPrefs(_memory);
        }

        public void SeedMemory(ProductionSnapshot snapshot) => _memory = Clone(snapshot);

        private ProductionSnapshot? LoadFromPrefs()
        {
#if UNITY_5_3_OR_NEWER || UNITY
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".meta"))
            {
                return null;
            }

            var snapshot = new ProductionSnapshot
            {
                SavedAtUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".meta"))
            };

            foreach (var pair in ProductionCatalog.All)
            {
                var accKey = _keyPrefix + ".b." + pair.Key + ".acc";
                var tsKey = _keyPrefix + ".b." + pair.Key + ".ts";
                if (!UnityEngine.PlayerPrefs.HasKey(accKey))
                {
                    continue;
                }

                snapshot.Buildings[pair.Key] = new BuildingProductionState(pair.Key, ParseTime(UnityEngine.PlayerPrefs.GetString(tsKey)))
                {
                    Accumulated = ReadLong(accKey)
                };
            }

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                var key = _keyPrefix + ".w." + resource;
                if (UnityEngine.PlayerPrefs.HasKey(key))
                {
                    snapshot.Wallet[resource] = ReadLong(key);
                }
            }

            return snapshot;
#else
            return null;
#endif
        }

        private void SaveToPrefs(ProductionSnapshot snapshot)
        {
#if UNITY_5_3_OR_NEWER || UNITY
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", CultureInfo.InvariantCulture));
            foreach (var pair in snapshot.Buildings)
            {
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".b." + pair.Key + ".acc", pair.Value.Accumulated.ToString(CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetString(
                    _keyPrefix + ".b." + pair.Key + ".ts",
                    pair.Value.LastUpdatedUtc.ToString("O", CultureInfo.InvariantCulture));
            }

            foreach (var pair in snapshot.Wallet)
            {
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".w." + pair.Key, pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            UnityEngine.PlayerPrefs.Save();
#endif
        }

#if UNITY_5_3_OR_NEWER || UNITY
        private static long ReadLong(string key)
        {
            var raw = UnityEngine.PlayerPrefs.GetString(key, "0");
            return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
        }
#endif

        private static DateTime ParseTime(string value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                : DateTime.UtcNow;
        }

        private static ProductionSnapshot Clone(ProductionSnapshot source)
        {
            var clone = new ProductionSnapshot { SavedAtUtc = source.SavedAtUtc };
            foreach (var pair in source.Buildings)
            {
                clone.Buildings[pair.Key] = new BuildingProductionState(pair.Value.BuildingDefinitionId, pair.Value.LastUpdatedUtc)
                {
                    Accumulated = pair.Value.Accumulated
                };
            }

            foreach (var pair in source.Wallet)
            {
                clone.Wallet[pair.Key] = pair.Value;
            }

            return clone;
        }
    }
}
