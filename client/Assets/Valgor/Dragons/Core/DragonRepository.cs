using System;
using System.Collections.Generic;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Core
{
    public sealed class DragonSnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public DragonRoost? Roost { get; set; }
        public Dictionary<string, DragonInstance> Dragons { get; set; } = new();
        public DragonEggJourneyPhase EggJourneyPhase { get; set; } = DragonEggJourneyPhase.Locked;
        public int SyncedCastleLevel { get; set; }
        public int SyncedTowerLevel { get; set; } = 1;
        public int PersistenceVersion { get; set; } = 5;
    }

    public interface IDragonRepository
    {
        DragonSnapshot? Load();
        void Save(DragonSnapshot snapshot);
    }

    /// <summary>
    /// Persistência técnica. Memória cobre City↔WorldMap; PlayerPrefs cobre restart.
    /// Migra automaticamente de valgor.dragons.v4 → v5.
    /// </summary>
    public sealed class DragonRepository : IDragonRepository
    {
        private readonly string _keyPrefix;
        private readonly string? _legacyKeyPrefix;
        private DragonSnapshot? _memory;

        public DragonRepository(string keyPrefix, string? legacyKeyPrefix = null)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
            _legacyKeyPrefix = legacyKeyPrefix;
        }

        public DragonSnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

#if UNITY_5_3_OR_NEWER
            var current = LoadFromPrefs(_keyPrefix);
            if (current != null)
            {
                return current;
            }

            if (!string.IsNullOrEmpty(_legacyKeyPrefix))
            {
                var legacy = LoadFromPrefs(_legacyKeyPrefix!);
                if (legacy != null)
                {
                    legacy.PersistenceVersion = 5;
                    MigratePhase2Defaults(legacy);
                    return legacy;
                }
            }

            return null;
#else
            return null;
#endif
        }

        public void Save(DragonSnapshot snapshot)
        {
            snapshot.PersistenceVersion = 5;
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(_keyPrefix, snapshot);
#endif
        }

        private static void MigratePhase2Defaults(DragonSnapshot snapshot)
        {
            foreach (var dragon in snapshot.Dragons.Values)
            {
                if (dragon.DragonLevel >= 1)
                {
                    if (dragon.Energy <= 0 && dragon.Health <= 0)
                    {
                        dragon.Energy = 100;
                        dragon.Health = 100;
                    }

                    if (dragon.GrowthStage < DragonProgressionRules.StageForLevel(dragon.DragonLevel))
                    {
                        dragon.GrowthStage = DragonProgressionRules.StageForLevel(dragon.DragonLevel);
                    }
                }
            }

            if (snapshot.SyncedTowerLevel <= 0)
            {
                snapshot.SyncedTowerLevel = 1;
            }
        }

        private static DragonSnapshot Clone(DragonSnapshot source)
        {
            var clone = new DragonSnapshot
            {
                SavedAtUtc = source.SavedAtUtc,
                EggJourneyPhase = source.EggJourneyPhase,
                SyncedCastleLevel = source.SyncedCastleLevel,
                SyncedTowerLevel = source.SyncedTowerLevel,
                PersistenceVersion = source.PersistenceVersion
            };
            if (source.Roost != null)
            {
                clone.Roost = new DragonRoost(
                    source.Roost.RoostId,
                    source.Roost.BuildingDefinitionId,
                    source.Roost.Capacity,
                    source.Roost.Level);
                clone.Roost.OccupantIds.AddRange(source.Roost.OccupantIds);
            }

            foreach (var pair in source.Dragons)
            {
                clone.Dragons[pair.Key] = pair.Value.Clone();
            }

            return clone;
        }

#if UNITY_5_3_OR_NEWER
        private static DragonSnapshot? LoadFromPrefs(string keyPrefix)
        {
            if (!UnityEngine.PlayerPrefs.HasKey(keyPrefix + ".meta"))
            {
                return null;
            }

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var snapshot = new DragonSnapshot
            {
                SavedAtUtc = DateTime.Parse(
                    UnityEngine.PlayerPrefs.GetString(keyPrefix + ".meta"),
                    inv,
                    System.Globalization.DateTimeStyles.RoundtripKind),
                EggJourneyPhase = (DragonEggJourneyPhase)UnityEngine.PlayerPrefs.GetInt(
                    keyPrefix + ".journey",
                    (int)DragonEggJourneyPhase.Locked),
                SyncedCastleLevel = UnityEngine.PlayerPrefs.GetInt(keyPrefix + ".castle", 0),
                SyncedTowerLevel = UnityEngine.PlayerPrefs.GetInt(keyPrefix + ".tower", 1),
                PersistenceVersion = UnityEngine.PlayerPrefs.GetInt(keyPrefix + ".ver", 4)
            };

            var roostId = UnityEngine.PlayerPrefs.GetString(keyPrefix + ".roost.id", string.Empty);
            if (!string.IsNullOrEmpty(roostId))
            {
                snapshot.Roost = new DragonRoost(
                    roostId,
                    UnityEngine.PlayerPrefs.GetString(keyPrefix + ".roost.building", "dragon-tower"),
                    UnityEngine.PlayerPrefs.GetInt(keyPrefix + ".roost.cap", 3),
                    UnityEngine.PlayerPrefs.GetInt(keyPrefix + ".roost.level", 1));
                var occupants = UnityEngine.PlayerPrefs.GetString(keyPrefix + ".roost.occ", string.Empty);
                if (!string.IsNullOrEmpty(occupants))
                {
                    snapshot.Roost.OccupantIds.AddRange(occupants.Split(','));
                }
            }

            var idsRaw = UnityEngine.PlayerPrefs.GetString(keyPrefix + ".ids", string.Empty);
            if (!string.IsNullOrEmpty(idsRaw))
            {
                foreach (var id in idsRaw.Split(','))
                {
                    var key = keyPrefix + ".d." + id;
                    DateTime? ends = null;
                    if (UnityEngine.PlayerPrefs.HasKey(key + ".ends"))
                    {
                        ends = DateTime.Parse(
                            UnityEngine.PlayerPrefs.GetString(key + ".ends"),
                            inv,
                            System.Globalization.DateTimeStyles.RoundtripKind);
                    }

                    DateTime? levelEnds = null;
                    if (UnityEngine.PlayerPrefs.HasKey(key + ".lvlends"))
                    {
                        levelEnds = DateTime.Parse(
                            UnityEngine.PlayerPrefs.GetString(key + ".lvlends"),
                            inv,
                            System.Globalization.DateTimeStyles.RoundtripKind);
                    }

                    var march = UnityEngine.PlayerPrefs.GetString(key + ".march", string.Empty);
                    snapshot.Dragons[id] = new DragonInstance(
                        id,
                        UnityEngine.PlayerPrefs.GetString(key + ".def"),
                        (DragonState)UnityEngine.PlayerPrefs.GetInt(key + ".state"),
                        UnityEngine.PlayerPrefs.GetInt(key + ".hunger"),
                        ends,
                        string.IsNullOrEmpty(march) ? null : march,
                        UnityEngine.PlayerPrefs.GetString(key + ".roost", string.Empty))
                    {
                        LastUpdatedUtc = DateTime.Parse(
                            UnityEngine.PlayerPrefs.GetString(key + ".updated"),
                            inv,
                            System.Globalization.DateTimeStyles.RoundtripKind),
                        GrowthStage = (DragonGrowthStage)UnityEngine.PlayerPrefs.GetInt(
                            key + ".growth",
                            (int)DragonGrowthStage.Egg),
                        GrowthPoints = UnityEngine.PlayerPrefs.GetInt(key + ".gpts", 0),
                        BondLevel = UnityEngine.PlayerPrefs.GetInt(key + ".bond", 0),
                        BondPoints = UnityEngine.PlayerPrefs.GetInt(key + ".bpts", 0),
                        DragonLevel = UnityEngine.PlayerPrefs.GetInt(key + ".lvl", 0),
                        CareCount = UnityEngine.PlayerPrefs.GetInt(key + ".care", 0),
                        Experience = UnityEngine.PlayerPrefs.GetInt(key + ".xp", 0),
                        Energy = UnityEngine.PlayerPrefs.GetInt(key + ".energy", 0),
                        Health = UnityEngine.PlayerPrefs.GetInt(key + ".health", 0),
                        IsLevelingUp = UnityEngine.PlayerPrefs.GetInt(key + ".lvling", 0) == 1,
                        PendingLevel = UnityEngine.PlayerPrefs.GetInt(key + ".pend", 0),
                        LevelUpEndsAtUtc = levelEnds
                    };
                }
            }

            MigratePhase2Defaults(snapshot);
            return snapshot;
        }

        private static void SaveToPrefs(string keyPrefix, DragonSnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetString(keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".journey", (int)snapshot.EggJourneyPhase);
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".castle", snapshot.SyncedCastleLevel);
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".tower", snapshot.SyncedTowerLevel);
            UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".ver", 5);
            if (snapshot.Roost == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(keyPrefix + ".roost.id");
            }
            else
            {
                UnityEngine.PlayerPrefs.SetString(keyPrefix + ".roost.id", snapshot.Roost.RoostId);
                UnityEngine.PlayerPrefs.SetString(keyPrefix + ".roost.building", snapshot.Roost.BuildingDefinitionId);
                UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".roost.cap", snapshot.Roost.Capacity);
                UnityEngine.PlayerPrefs.SetInt(keyPrefix + ".roost.level", snapshot.Roost.Level);
                UnityEngine.PlayerPrefs.SetString(
                    keyPrefix + ".roost.occ",
                    string.Join(",", snapshot.Roost.OccupantIds));
            }

            UnityEngine.PlayerPrefs.SetString(keyPrefix + ".ids", string.Join(",", snapshot.Dragons.Keys));
            foreach (var pair in snapshot.Dragons)
            {
                var key = keyPrefix + ".d." + pair.Key;
                var d = pair.Value;
                UnityEngine.PlayerPrefs.SetString(key + ".def", d.DefinitionId);
                UnityEngine.PlayerPrefs.SetInt(key + ".state", (int)d.State);
                UnityEngine.PlayerPrefs.SetInt(key + ".hunger", d.Hunger);
                UnityEngine.PlayerPrefs.SetString(key + ".updated", d.LastUpdatedUtc.ToString("O", inv));
                UnityEngine.PlayerPrefs.SetString(key + ".march", d.AssignedMarchId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetString(key + ".roost", d.RoostId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetInt(key + ".growth", (int)d.GrowthStage);
                UnityEngine.PlayerPrefs.SetInt(key + ".gpts", d.GrowthPoints);
                UnityEngine.PlayerPrefs.SetInt(key + ".bond", d.BondLevel);
                UnityEngine.PlayerPrefs.SetInt(key + ".bpts", d.BondPoints);
                UnityEngine.PlayerPrefs.SetInt(key + ".lvl", d.DragonLevel);
                UnityEngine.PlayerPrefs.SetInt(key + ".care", d.CareCount);
                UnityEngine.PlayerPrefs.SetInt(key + ".xp", d.Experience);
                UnityEngine.PlayerPrefs.SetInt(key + ".energy", d.Energy);
                UnityEngine.PlayerPrefs.SetInt(key + ".health", d.Health);
                UnityEngine.PlayerPrefs.SetInt(key + ".lvling", d.IsLevelingUp ? 1 : 0);
                UnityEngine.PlayerPrefs.SetInt(key + ".pend", d.PendingLevel);
                if (d.StateEndsAtUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".ends", d.StateEndsAtUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".ends");
                }

                if (d.LevelUpEndsAtUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".lvlends", d.LevelUpEndsAtUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".lvlends");
                }
            }

            UnityEngine.PlayerPrefs.Save();
        }
#endif
    }
}
