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
    }

    public interface IDragonRepository
    {
        DragonSnapshot? Load();
        void Save(DragonSnapshot snapshot);
    }

    /// <summary>
    /// Persistência técnica. Memória cobre City↔WorldMap; PlayerPrefs cobre restart.
    /// </summary>
    public sealed class DragonRepository : IDragonRepository
    {
        private readonly string _keyPrefix;
        private DragonSnapshot? _memory;

        public DragonRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public DragonSnapshot? Load()
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

        public void Save(DragonSnapshot snapshot)
        {
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(snapshot);
#endif
        }

        private static DragonSnapshot Clone(DragonSnapshot source)
        {
            var clone = new DragonSnapshot { SavedAtUtc = source.SavedAtUtc };
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
        private DragonSnapshot? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".meta"))
            {
                return null;
            }

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var snapshot = new DragonSnapshot
            {
                SavedAtUtc = DateTime.Parse(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".meta"),
                    inv,
                    System.Globalization.DateTimeStyles.RoundtripKind)
            };

            var roostId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".roost.id", string.Empty);
            if (!string.IsNullOrEmpty(roostId))
            {
                snapshot.Roost = new DragonRoost(
                    roostId,
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".roost.building", "dragon-tower"),
                    UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".roost.cap", 3),
                    UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".roost.level", 1));
                var occupants = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".roost.occ", string.Empty);
                if (!string.IsNullOrEmpty(occupants))
                {
                    snapshot.Roost.OccupantIds.AddRange(occupants.Split(','));
                }
            }

            var idsRaw = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".ids", string.Empty);
            if (!string.IsNullOrEmpty(idsRaw))
            {
                foreach (var id in idsRaw.Split(','))
                {
                    var key = _keyPrefix + ".d." + id;
                    DateTime? ends = null;
                    if (UnityEngine.PlayerPrefs.HasKey(key + ".ends"))
                    {
                        ends = DateTime.Parse(
                            UnityEngine.PlayerPrefs.GetString(key + ".ends"),
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
                        BondPoints = UnityEngine.PlayerPrefs.GetInt(key + ".bpts", 0)
                    };
                }
            }

            return snapshot;
        }

        private void SaveToPrefs(DragonSnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", inv));
            if (snapshot.Roost == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + ".roost.id");
            }
            else
            {
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".roost.id", snapshot.Roost.RoostId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".roost.building", snapshot.Roost.BuildingDefinitionId);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".roost.cap", snapshot.Roost.Capacity);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".roost.level", snapshot.Roost.Level);
                UnityEngine.PlayerPrefs.SetString(
                    _keyPrefix + ".roost.occ",
                    string.Join(",", snapshot.Roost.OccupantIds));
            }

            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".ids", string.Join(",", snapshot.Dragons.Keys));
            foreach (var pair in snapshot.Dragons)
            {
                var key = _keyPrefix + ".d." + pair.Key;
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
                if (d.StateEndsAtUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".ends", d.StateEndsAtUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".ends");
                }
            }

            UnityEngine.PlayerPrefs.Save();
        }
#endif
    }
}
