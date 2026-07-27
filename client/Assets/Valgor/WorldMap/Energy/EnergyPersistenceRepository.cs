using System;
using System.Globalization;

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
        public const string DefaultKeyPrefix = "valgor.worldmap.energy.v1";

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

        /// <summary>Limpa prefs de energia (Novo Jogo / migração segura).</summary>
        public static void ClearPrefs(string keyPrefix = DefaultKeyPrefix)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".current");
            UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".max");
            UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".updated");
            UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".interval");
            UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".regen");
            UnityEngine.PlayerPrefs.Save();
#endif
        }

        /// <summary>Seed completo (nunca só current/max — evita FormatException no load).</summary>
        public static void SeedDefaults(
            string keyPrefix = DefaultKeyPrefix,
            int current = 100,
            int max = 100,
            double intervalSec = 60,
            int regenAmount = 1)
        {
#if UNITY_5_3_OR_NEWER
            var inv = CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".current", current);
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".max", max);
            UnityEngine.PlayerPrefs.SetString(keyPrefix + ".updated", DateTime.UtcNow.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(keyPrefix + ".interval", intervalSec.ToString(inv));
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".regen", regenAmount);
            UnityEngine.PlayerPrefs.Save();
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

            var inv = CultureInfo.InvariantCulture;
            var updatedRaw = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".updated", string.Empty);
            if (!TryParseUtc(updatedRaw, out var updatedAt))
            {
                // Prefs parciais (ex.: Seed antigo só com current/max) — recupera sem derrubar o mapa.
                updatedAt = DateTime.UtcNow;
                UnityEngine.Debug.LogWarning(
                    $"[Valgor] Energia: timestamp inválido ('{updatedRaw}'). Usando UtcNow e regravando prefs.");
            }

            var intervalRaw = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".interval", "60");
            if (!double.TryParse(intervalRaw, NumberStyles.Float, inv, out var interval) || interval <= 0)
            {
                interval = 60;
            }

            var max = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".max", 100);
            if (max <= 0)
            {
                max = 100;
            }

            var current = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".current", max);
            current = Math.Clamp(current, 0, max);

            var snapshot = new EnergySnapshot
            {
                CurrentEnergy = current,
                MaxEnergy = max,
                LastUpdatedAt = updatedAt,
                RegenIntervalSec = interval,
                RegenAmount = Math.Max(1, UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".regen", 1))
            };

            // Normaliza prefs corrompidos/parciais para a próxima abertura.
            SaveToPrefs(snapshot);
            return snapshot;
        }

        private void SaveToPrefs(EnergySnapshot snapshot)
        {
            var inv = CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".current", snapshot.CurrentEnergy);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".max", snapshot.MaxEnergy);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".updated", snapshot.LastUpdatedAt.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".interval", snapshot.RegenIntervalSec.ToString(inv));
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".regen", snapshot.RegenAmount);
            UnityEngine.PlayerPrefs.Save();
        }

        private static bool TryParseUtc(string raw, out DateTime utc)
        {
            utc = default;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (DateTime.TryParse(
                    raw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces,
                    out utc))
            {
                if (utc.Kind == DateTimeKind.Unspecified)
                {
                    utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
                }
                else if (utc.Kind == DateTimeKind.Local)
                {
                    utc = utc.ToUniversalTime();
                }

                return true;
            }

            // Tentativa extra: cultura atual (prefs antigos mal formatados).
            if (DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out utc))
            {
                utc = utc.ToUniversalTime();
                return true;
            }

            return false;
        }
#endif
    }
}
