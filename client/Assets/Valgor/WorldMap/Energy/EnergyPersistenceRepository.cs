using System;

namespace Valgor.WorldMap.Energy
{
    public sealed class EnergySnapshot
    {
        public int CurrentEnergy { get; set; }
        public int MaxEnergy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public double RegenIntervalSec { get; set; }
        public int RegenAmount { get; set; }
    }

    public interface IEnergyPersistenceRepository
    {
        EnergySnapshot? Load();
        void Save(EnergySnapshot snapshot);
    }

    /// <summary>
    /// Persistência técnica de energia. Memória da instância cobre City↔WorldMap;
    /// PlayerPrefs cobre restart. Contrato pronto para backend.
    /// </summary>
    public sealed class EnergyPersistenceRepository : IEnergyPersistenceRepository
    {
        private readonly string _keyPrefix;
        private EnergySnapshot? _memory;

        public EnergyPersistenceRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public EnergySnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

#if UNITY_5_3_OR_NEWER
            return LoadFromPrefs();
#else
            return null;
#endif
        }

        public void Save(EnergySnapshot snapshot)
        {
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(snapshot);
#endif
        }

        private static EnergySnapshot Clone(EnergySnapshot source) =>
            new()
            {
                CurrentEnergy = source.CurrentEnergy,
                MaxEnergy = source.MaxEnergy,
                LastUpdatedAt = source.LastUpdatedAt,
                RegenIntervalSec = source.RegenIntervalSec,
                RegenAmount = source.RegenAmount
            };

#if UNITY_5_3_OR_NEWER
        private EnergySnapshot? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".current"))
            {
                return null;
            }

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            return new EnergySnapshot
            {
                CurrentEnergy = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".current"),
                MaxEnergy = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".max"),
                LastUpdatedAt = DateTime.Parse(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".updated"),
                    inv,
                    System.Globalization.DateTimeStyles.RoundtripKind),
                RegenIntervalSec = double.Parse(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".interval", "60"),
                    inv),
                RegenAmount = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".regen", 1)
            };
        }

        private void SaveToPrefs(EnergySnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".current", snapshot.CurrentEnergy);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".max", snapshot.MaxEnergy);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".updated", snapshot.LastUpdatedAt.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".interval", snapshot.RegenIntervalSec.ToString(inv));
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".regen", snapshot.RegenAmount);
            UnityEngine.PlayerPrefs.Save();
        }
#endif
    }
}
