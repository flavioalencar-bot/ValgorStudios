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
        public string? SelectedNodeId { get; set; }
        public Dictionary<string, WorldNodeInstance> Nodes { get; } = new();
        public Dictionary<string, WorldCreatureInstance> Creatures { get; } = new();
        public MarchOrder? March { get; set; }
        public MarchOrder? LastCompletedMarch { get; set; }
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
                SelectedNodeId = source.SelectedNodeId,
                March = source.March?.Clone(),
                LastCompletedMarch = source.LastCompletedMarch?.Clone()
            };

            foreach (var pair in source.Nodes)
            {
                clone.Nodes[pair.Key] = new WorldNodeInstance(
                    pair.Value.DefinitionId,
                    pair.Value.Status,
                    pair.Value.RemainingAmount)
                {
                    OccupiedByMarchId = pair.Value.OccupiedByMarchId,
                    RespawnAt = pair.Value.RespawnAt,
                    LastGatherUpdatedUtc = pair.Value.LastGatherUpdatedUtc,
                    ResourceState = pair.Value.ResourceState
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
                Energy = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".energy", 0),
                SelectedNodeId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".selected", string.Empty))
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
                DateTime? respawnAt = null;
                if (UnityEngine.PlayerPrefs.HasKey(key + ".respawnAt"))
                {
                    respawnAt = ParseTime(UnityEngine.PlayerPrefs.GetString(key + ".respawnAt"));
                }

                DateTime? lastGather = null;
                if (UnityEngine.PlayerPrefs.HasKey(key + ".lastGather"))
                {
                    lastGather = ParseTime(UnityEngine.PlayerPrefs.GetString(key + ".lastGather"));
                }

                snapshot.Nodes[definition.Id] = new WorldNodeInstance(definition.Id, status, amount)
                {
                    OccupiedByMarchId = string.IsNullOrEmpty(occupied) ? null : occupied,
                    RespawnAt = respawnAt,
                    LastGatherUpdatedUtc = lastGather,
                    ResourceState = UnityEngine.PlayerPrefs.HasKey(key + ".rstate")
                        ? (ResourceNodeState)UnityEngine.PlayerPrefs.GetInt(key + ".rstate")
                        : MapResourceState(status)
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
                snapshot.March = ReadMarchFromPrefs(".march");
            }

            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".done.id"))
            {
                snapshot.LastCompletedMarch = ReadMarchFromPrefs(".done");
            }

            return snapshot;
        }

        private MarchOrder ReadMarchFromPrefs(string suffix)
        {
            DateTime? returnAt = null;
            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + suffix + ".ret"))
            {
                returnAt = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".ret"));
            }

            DateTime? deliveredAt = null;
            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + suffix + ".deliveredAt"))
            {
                deliveredAt = ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".deliveredAt"));
            }

            return new MarchOrder(
                UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".id"),
                UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".player"),
                UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".origin"),
                UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".target"),
                UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".team"),
                ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".dep")),
                ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".arr")),
                (MarchState)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + suffix + ".state"),
                float.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".speed", "8"), System.Globalization.CultureInfo.InvariantCulture),
                long.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".cap", "10000"), System.Globalization.CultureInfo.InvariantCulture),
                (WorldNodeKind)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + suffix + ".type"),
                returnAt,
                long.Parse(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".load", "0"), System.Globalization.CultureInfo.InvariantCulture))
            {
                RewardsDelivered = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + suffix + ".rewarded", 0) == 1,
                OccupyingNodeId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".occ", string.Empty)),
                RewardDeliveryId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix + ".deliveryId", string.Empty)),
                DeliveredAt = deliveredAt,
                IsCommitted = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + suffix + ".committed", 0) == 1
            };
        }

        private void SaveToPrefs(WorldMapSnapshot snapshot)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".meta", snapshot.SavedAtUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".advance", snapshot.LastAdvanceUtc.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".energy", snapshot.Energy);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".selected", snapshot.SelectedNodeId ?? string.Empty);

            foreach (var pair in snapshot.Nodes)
            {
                var key = _keyPrefix + ".n." + pair.Key;
                UnityEngine.PlayerPrefs.SetInt(key + ".status", (int)pair.Value.Status);
                UnityEngine.PlayerPrefs.SetString(key + ".amount", pair.Value.RemainingAmount.ToString(inv));
                UnityEngine.PlayerPrefs.SetString(key + ".occ", pair.Value.OccupiedByMarchId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetInt(key + ".rstate", (int)pair.Value.ResourceState);
                if (pair.Value.RespawnAt.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".respawnAt", pair.Value.RespawnAt.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".respawnAt");
                }

                if (pair.Value.LastGatherUpdatedUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".lastGather", pair.Value.LastGatherUpdatedUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".lastGather");
                }
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
                DeleteMarchPrefs(".march");
            }
            else
            {
                WriteMarchToPrefs(".march", snapshot.March, inv);
            }

            if (snapshot.LastCompletedMarch == null)
            {
                DeleteMarchPrefs(".done");
            }
            else
            {
                WriteMarchToPrefs(".done", snapshot.LastCompletedMarch, inv);
            }

            UnityEngine.PlayerPrefs.Save();
        }

        private void WriteMarchToPrefs(string suffix, MarchOrder m, System.Globalization.CultureInfo inv)
        {
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".id", m.MarchId);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".player", m.PlayerId);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".origin", m.OriginNodeId);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".target", m.TargetNodeId);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".team", m.SelectedTeamId);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".dep", m.DepartureAt.ToString("O", inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".arr", m.ArrivalAt.ToString("O", inv));
            if (m.ReturnAt.HasValue)
            {
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".ret", m.ReturnAt.Value.ToString("O", inv));
            }
            else
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".ret");
            }

            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + suffix + ".state", (int)m.State);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".speed", m.Speed.ToString(inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".cap", m.Capacity.ToString(inv));
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".load", m.ResourceLoad.ToString(inv));
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + suffix + ".type", (int)m.TargetType);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + suffix + ".rewarded", m.RewardsDelivered ? 1 : 0);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".occ", m.OccupyingNodeId ?? string.Empty);
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".deliveryId", m.RewardDeliveryId ?? string.Empty);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + suffix + ".committed", m.IsCommitted ? 1 : 0);
            if (m.DeliveredAt.HasValue)
            {
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix + ".deliveredAt", m.DeliveredAt.Value.ToString("O", inv));
            }
            else
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".deliveredAt");
            }
        }

        private void DeleteMarchPrefs(string suffix)
        {
            UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".id");
            UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".deliveryId");
            UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".committed");
            UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + suffix + ".deliveredAt");
        }

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static ResourceNodeState MapResourceState(WorldNodeStatus status) => status switch
        {
            WorldNodeStatus.Occupied => ResourceNodeState.Occupied,
            WorldNodeStatus.Depleted => ResourceNodeState.Depleted,
            WorldNodeStatus.Respawning => ResourceNodeState.Respawning,
            _ => ResourceNodeState.Available
        };

        private static DateTime ParseTime(string raw) =>
            DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
#endif
    }
}
