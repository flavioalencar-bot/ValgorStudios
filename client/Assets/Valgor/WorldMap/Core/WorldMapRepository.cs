using System;
using System.Collections.Generic;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldMapSnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public DateTime LastAdvanceUtc { get; set; }
        public int Energy { get; set; }
        public Dictionary<string, WorldNodeInstance> Nodes { get; } = new();
        public Dictionary<string, WorldCreatureInstance> Creatures { get; } = new();
        public MarchOrder? March { get; set; }
    }

    public interface IWorldMapRepository
    {
        WorldMapSnapshot? Load();
        void Save(WorldMapSnapshot snapshot);
    }

    /// <summary>
    /// Persistência local técnica. Memória da instância cobre City↔WorldMap via ServiceRegistry;
    /// PlayerPrefs cobre restart. Contrato pronto para backend.
    /// </summary>
    public sealed class LocalWorldMapRepository : IWorldMapRepository
    {
        private readonly string _keyPrefix;
        private WorldMapSnapshot? _memory;

        public LocalWorldMapRepository(string keyPrefix)
        {
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        }

        public WorldMapSnapshot? Load()
        {
            if (_memory != null)
            {
                return Clone(_memory);
            }

#if UNITY_5_3_OR_NEWER
            var loaded = LoadFromPrefs();
            if (loaded != null)
            {
                _memory = Clone(loaded);
            }

            return loaded;
#else
            return null;
#endif
        }

        public void Save(WorldMapSnapshot snapshot)
        {
            _memory = Clone(snapshot);
#if UNITY_5_3_OR_NEWER
            SaveToPrefs(snapshot);
#endif
        }

        private static WorldMapSnapshot Clone(WorldMapSnapshot source)
        {
            var clone = new WorldMapSnapshot
            {
                SavedAtUtc = source.SavedAtUtc,
                LastAdvanceUtc = source.LastAdvanceUtc,
                Energy = source.Energy,
                March = source.March?.Clone()
            };

            foreach (var pair in source.Nodes)
            {
                clone.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount)
                {
                    OccupiedByMarchId = pair.Value.OccupiedByMarchId
                };
            }

            foreach (var pair in source.Creatures)
            {
                clone.Creatures[pair.Key] = new WorldCreatureInstance(
                    pair.Value.DefinitionId,
                    pair.Value.State,
                    pair.Value.RegionId,
                    pair.Value.X,
                    pair.Value.Z,
                    pair.Value.RespawnAtUtc)
                {
                    EngagedMarchId = pair.Value.EngagedMarchId
                };
            }

            return clone;
        }

#if UNITY_5_3_OR_NEWER
        private WorldMapSnapshot? LoadFromPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".meta"))
            {
                return null;
            }

            var snapshot = new WorldMapSnapshot
            {
                SavedAtUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".meta")),
                LastAdvanceUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".advance")),
                Energy = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".energy", 0)
            };

            foreach (var definition in WorldNodeCatalog.All.Values)
            {
                var key = _keyPrefix + ".n." + definition.Id;
                if (!UnityEngine.PlayerPrefs.HasKey(key + ".status"))
                {
                    continue;
                }

                var status = (WorldNodeStatus)UnityEngine.PlayerPrefs.GetInt(key + ".status");
                var amount = long.Parse(
                    UnityEngine.PlayerPrefs.GetString(key + ".amount", "0"),
                    System.Globalization.CultureInfo.InvariantCulture);
                var occupied = UnityEngine.PlayerPrefs.GetString(key + ".occ", string.Empty);
                snapshot.Nodes[definition.Id] = new WorldNodeInstance(definition.Id, status, amount)
                {
                    OccupiedByMarchId = string.IsNullOrEmpty(occupied) ? null : occupied
                };
            }

            foreach (var definition in WorldCreatureCatalog.All.Values)
            {
                var key = _keyPrefix + ".c." + definition.Id;
                if (!UnityEngine.PlayerPrefs.HasKey(key + ".state"))
                {
                    continue;
                }

                DateTime? respawn = null;
                if (UnityEngine.PlayerPrefs.HasKey(key + ".respawn"))
                {
                    respawn = ParseTime(UnityEngine.PlayerPrefs.GetString(key + ".respawn"));
                }

                snapshot.Creatures[definition.Id] = new WorldCreatureInstance(
                    definition.Id,
                    (WorldCreatureState)UnityEngine.PlayerPrefs.GetInt(key + ".state"),
                    definition.RegionId,
                    definition.X,
                    definition.Z,
                    respawn)
                {
                    EngagedMarchId = UnityEngine.PlayerPrefs.GetString(key + ".march", string.Empty)
                };
                if (string.IsNullOrEmpty(snapshot.Creatures[definition.Id].EngagedMarchId))
                {
                    snapshot.Creatures[definition.Id].EngagedMarchId = null;
                }
            }

            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".march.id"))
            {
                DateTime? returnAt = null;
                if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".march.ret"))
                {
                    returnAt = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.ret"));
                }

                var march = new MarchOrder(
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.id"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.player"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.origin"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.target"),
                    UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.team"),
                    ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.dep")),
                    ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.arr")),
                    (MarchState)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".march.state"),
                    float.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.speed", "8"), System.Globalization.CultureInfo.InvariantCulture),
                    long.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.cap", "10000"), System.Globalization.CultureInfo.InvariantCulture),
                    (WorldNodeKind)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".march.type"),
                    returnAt,
                    long.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.load", "0"), System.Globalization.CultureInfo.InvariantCulture))
                {
                    RewardsDelivered = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".march.rewarded", 0) == 1,
                    OccupyingNodeId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".march.occ", string.Empty))
                };
                snapshot.March = march;
            }

            return snapshot;
        }

        private void SaveToPrefs(WorldMapSnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".advance", snapshot.LastAdvanceUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".energy", snapshot.Energy);

            foreach (var pair in snapshot.Nodes)
            {
                var key = _keyPrefix + ".n." + pair.Key;
                UnityEngine.PlayerPrefs.SetInt(key + ".status", (int)pair.Value.Status);
                UnityEngine.PlayerPrefs.SetString(key + ".amount", pair.Value.RemainingAmount.ToString(inv));
                UnityEngine.PlayerPrefs.SetString(key + ".occ", pair.Value.OccupiedByMarchId ?? string.Empty);
            }

            foreach (var pair in snapshot.Creatures)
            {
                var key = _keyPrefix + ".c." + pair.Key;
                UnityEngine.PlayerPrefs.SetInt(key + ".state", (int)pair.Value.State);
                UnityEngine.PlayerPrefs.SetString(key + ".march", pair.Value.EngagedMarchId ?? string.Empty);
                if (pair.Value.RespawnAtUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".respawn", pair.Value.RespawnAtUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".respawn");
                }
            }

            if (snapshot.March == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + ".march.id");
            }
            else
            {
                var m = snapshot.March;
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.id", m.MarchId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.player", m.PlayerId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.origin", m.OriginNodeId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.target", m.TargetNodeId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.team", m.SelectedTeamId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.dep", m.DepartureAt.ToString("O", inv));
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.arr", m.ArrivalAt.ToString("O", inv));
                if (m.ReturnAt.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.ret", m.ReturnAt.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + ".march.ret");
                }

                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".march.state", (int)m.State);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.speed", m.Speed.ToString(inv));
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.cap", m.Capacity.ToString(inv));
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.load", m.ResourceLoad.ToString(inv));
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".march.type", (int)m.TargetType);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".march.rewarded", m.RewardsDelivered ? 1 : 0);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".march.occ", m.OccupyingNodeId ?? string.Empty);
            }

            UnityEngine.PlayerPrefs.Save();
        }

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static DateTime ParseTime(string raw) =>
            DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
#endif
    }
}
