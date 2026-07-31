using System;
using System.Collections.Generic;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Marches;
using Valgor.WorldMap.Raids;

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
        public Dictionary<string, CityRaidInstance> Raids { get; } = new();
        public MarchOrder? March { get; set; }
        public MarchOrder? LastCompletedMarch { get; set; }
        public CityBattleReport? LastBattleReport { get; set; }
        /// <summary>Fase 5: IDs processados (batalha/saque/relatório/defesa).</summary>
        public ProcessedOperationRegistry ProcessedOperations { get; set; } = new();
        /// <summary>Fase 5: eventos de invasão NPC pendentes/concluídos.</summary>
        public List<DefenseEvent> DefenseEvents { get; set; } = new();
        public int BoundWallLevel { get; set; }
        public int BoundHospitalLevel { get; set; }
        public int BoundWarehouseLevel { get; set; }
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
                LastCompletedMarch = source.LastCompletedMarch?.Clone(),
                LastBattleReport = CloneReport(source.LastBattleReport),
                ProcessedOperations = source.ProcessedOperations?.Clone() ?? new ProcessedOperationRegistry(),
                BoundWallLevel = source.BoundWallLevel,
                BoundHospitalLevel = source.BoundHospitalLevel,
                BoundWarehouseLevel = source.BoundWarehouseLevel
            };
            if (source.DefenseEvents != null)
            {
                foreach (var evt in source.DefenseEvents)
                {
                    clone.DefenseEvents.Add(LocalDefenseScheduler.CloneEvent(evt));
                }
            }

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

            foreach (var pair in source.Raids)
            {
                clone.Raids[pair.Key] = pair.Value.Clone();
            }

            return clone;
        }

        private static CityBattleReport? CloneReport(CityBattleReport? source)
        {
            if (source == null)
            {
                return null;
            }

            return new CityBattleReport
            {
                CreatedUtc = source.CreatedUtc,
                TargetCityId = source.TargetCityId,
                TargetDisplayName = source.TargetDisplayName,
                Victory = source.Victory,
                IsDefense = source.IsDefense,
                AttackerPower = source.AttackerPower,
                DefensePower = source.DefensePower,
                Band = source.Band,
                DragonRoleLabel = source.DragonRoleLabel,
                TroopWoundedPercent = source.TroopWoundedPercent,
                LootGranted = source.LootGranted,
                DragonSummary = source.DragonSummary,
                Summary = source.Summary,
                BattleId = source.BattleId,
                ReportId = source.ReportId,
                WallDurabilityAfter = source.WallDurabilityAfter,
                DominatorWrathUsed = source.DominatorWrathUsed,
                DefenseEventId = source.DefenseEventId
            };
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

            foreach (var definition in CityRaidCatalog.All.Values)
            {
                var key = _keyPrefix + ".raid." + definition.CityNodeId;
                if (!UnityEngine.PlayerPrefs.HasKey(key + ".state"))
                {
                    continue;
                }

                DateTime? recover = null;
                if (UnityEngine.PlayerPrefs.HasKey(key + ".recover"))
                {
                    recover = ParseTime(UnityEngine.PlayerPrefs.GetString(key + ".recover"));
                }

                var march = UnityEngine.PlayerPrefs.GetString(key + ".march", string.Empty);
                snapshot.Raids[definition.CityNodeId] = new CityRaidInstance(
                    definition.CityNodeId,
                    (CityRaidState)UnityEngine.PlayerPrefs.GetInt(key + ".state"))
                {
                    EngagedMarchId = string.IsNullOrEmpty(march) ? null : march,
                    RecoverAtUtc = recover,
                    WallCurrentDurability = long.Parse(
                        UnityEngine.PlayerPrefs.GetString(key + ".wall", "0"),
                        System.Globalization.CultureInfo.InvariantCulture),
                    ActiveBattleId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(key + ".battle", string.Empty)),
                    ActiveLootTransactionId = NullIfEmpty(UnityEngine.PlayerPrefs.GetString(key + ".lootTx", string.Empty))
                };
            }

            LoadProcessedPrefs(snapshot);
            LoadDefenseEventsPrefs(snapshot);

            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".bound.wall"))
            {
                snapshot.BoundWallLevel = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".bound.wall", 0);
                snapshot.BoundHospitalLevel = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".bound.hospital", 0);
                snapshot.BoundWarehouseLevel = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".bound.warehouse", 0);
            }

            if (UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".report.summary"))
            {
                snapshot.LastBattleReport = new CityBattleReport
                {
                    CreatedUtc = UnityEngine.PlayerPrefs.HasKey(_keyPrefix + ".report.at")
                        ? ParseTime(UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.at"))
                        : DateTime.UtcNow,
                    TargetCityId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.city", string.Empty),
                    TargetDisplayName = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.name", string.Empty),
                    Victory = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.win", 0) == 1,
                    IsDefense = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.def", 0) == 1,
                    AttackerPower = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.atk", 0),
                    DefensePower = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.dp", 0),
                    Band = (CreatureDifficultyBand)UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.band", 0),
                    DragonRoleLabel = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.role", "Support"),
                    TroopWoundedPercent = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.wound", 0),
                    LootGranted = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.loot", 0) == 1,
                    DragonSummary = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.dsum", string.Empty),
                    Summary = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.summary", string.Empty),
                    BattleId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.bid", string.Empty),
                    ReportId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.rid", string.Empty),
                    WallDurabilityAfter = long.Parse(
                        UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.wallAfter", "0"),
                        System.Globalization.CultureInfo.InvariantCulture),
                    DominatorWrathUsed = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".report.wrath", 0) == 1,
                    DefenseEventId = UnityEngine.PlayerPrefs.GetString(_keyPrefix + ".report.deid", string.Empty)
                };
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

            foreach (var pair in snapshot.Raids)
            {
                var key = _keyPrefix + ".raid." + pair.Key;
                UnityEngine.PlayerPrefs.SetInt(key + ".state", (int)pair.Value.State);
                UnityEngine.PlayerPrefs.SetString(key + ".march", pair.Value.EngagedMarchId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetString(
                    key + ".wall",
                    pair.Value.WallCurrentDurability.ToString(System.Globalization.CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetString(key + ".battle", pair.Value.ActiveBattleId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetString(key + ".lootTx", pair.Value.ActiveLootTransactionId ?? string.Empty);
                if (pair.Value.RecoverAtUtc.HasValue)
                {
                    UnityEngine.PlayerPrefs.SetString(key + ".recover", pair.Value.RecoverAtUtc.Value.ToString("O", inv));
                }
                else
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key + ".recover");
                }
            }

            SaveProcessedPrefs(snapshot);
            SaveDefenseEventsPrefs(snapshot);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".bound.wall", snapshot.BoundWallLevel);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".bound.hospital", snapshot.BoundHospitalLevel);
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".bound.warehouse", snapshot.BoundWarehouseLevel);

            if (snapshot.LastBattleReport == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(_keyPrefix + ".report.summary");
            }
            else
            {
                var r = snapshot.LastBattleReport;
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.at", r.CreatedUtc.ToString("O", inv));
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.city", r.TargetCityId);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.name", r.TargetDisplayName);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.win", r.Victory ? 1 : 0);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.def", r.IsDefense ? 1 : 0);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.atk", r.AttackerPower);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.dp", r.DefensePower);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.band", (int)r.Band);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.role", r.DragonRoleLabel);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.wound", r.TroopWoundedPercent);
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.loot", r.LootGranted ? 1 : 0);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.dsum", r.DragonSummary);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.summary", r.Summary);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.bid", r.BattleId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.rid", r.ReportId ?? string.Empty);
                UnityEngine.PlayerPrefs.SetString(
                    _keyPrefix + ".report.wallAfter",
                    r.WallDurabilityAfter.ToString(System.Globalization.CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".report.wrath", r.DominatorWrathUsed ? 1 : 0);
                UnityEngine.PlayerPrefs.SetString(_keyPrefix + ".report.deid", r.DefenseEventId ?? string.Empty);
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

        private void LoadProcessedPrefs(WorldMapSnapshot snapshot)
        {
            snapshot.ProcessedOperations = new ProcessedOperationRegistry();
            FillIds(snapshot.ProcessedOperations.RaidIds, ".ops.raids");
            FillIds(snapshot.ProcessedOperations.MarchIds, ".ops.marches");
            FillIds(snapshot.ProcessedOperations.BattleIds, ".ops.battles");
            FillIds(snapshot.ProcessedOperations.LootTransactionIds, ".ops.loots");
            FillIds(snapshot.ProcessedOperations.ReportIds, ".ops.reports");
            FillIds(snapshot.ProcessedOperations.WallRepairIds, ".ops.repairs");
            FillIds(snapshot.ProcessedOperations.DefenseEventIds, ".ops.defenses");
        }

        private void SaveProcessedPrefs(WorldMapSnapshot snapshot)
        {
            var ops = snapshot.ProcessedOperations ?? new ProcessedOperationRegistry();
            WriteIds(ops.RaidIds, ".ops.raids");
            WriteIds(ops.MarchIds, ".ops.marches");
            WriteIds(ops.BattleIds, ".ops.battles");
            WriteIds(ops.LootTransactionIds, ".ops.loots");
            WriteIds(ops.ReportIds, ".ops.reports");
            WriteIds(ops.WallRepairIds, ".ops.repairs");
            WriteIds(ops.DefenseEventIds, ".ops.defenses");
        }

        private void FillIds(HashSet<string> target, string suffix)
        {
            var raw = UnityEngine.PlayerPrefs.GetString(_keyPrefix + suffix, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            foreach (var part in raw.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    target.Add(part);
                }
            }
        }

        private void WriteIds(HashSet<string> source, string suffix) =>
            UnityEngine.PlayerPrefs.SetString(_keyPrefix + suffix, string.Join("|", source));

        private void LoadDefenseEventsPrefs(WorldMapSnapshot snapshot)
        {
            snapshot.DefenseEvents = new List<DefenseEvent>();
            var count = UnityEngine.PlayerPrefs.GetInt(_keyPrefix + ".defevt.count", 0);
            for (var i = 0; i < count; i++)
            {
                var p = _keyPrefix + ".defevt." + i;
                if (!UnityEngine.PlayerPrefs.HasKey(p + ".id"))
                {
                    continue;
                }

                snapshot.DefenseEvents.Add(new DefenseEvent
                {
                    DefenseEventId = UnityEngine.PlayerPrefs.GetString(p + ".id"),
                    BattleId = UnityEngine.PlayerPrefs.GetString(p + ".battle"),
                    ReportId = UnityEngine.PlayerPrefs.GetString(p + ".report"),
                    ScheduledAtUtc = ParseTime(UnityEngine.PlayerPrefs.GetString(p + ".at")),
                    Status = (DefenseEventStatus)UnityEngine.PlayerPrefs.GetInt(p + ".status"),
                    ResultApplied = UnityEngine.PlayerPrefs.GetInt(p + ".applied", 0) == 1,
                    Attacker = new NpcAttackerSnapshot
                    {
                        AttackerId = UnityEngine.PlayerPrefs.GetString(p + ".atk.id", "npc-raider"),
                        DisplayName = UnityEngine.PlayerPrefs.GetString(p + ".atk.name", "Invasor NPC"),
                        Power = UnityEngine.PlayerPrefs.GetInt(p + ".atk.power", 1),
                        TroopCount = UnityEngine.PlayerPrefs.GetInt(p + ".atk.troops", 1)
                    }
                });
            }
        }

        private void SaveDefenseEventsPrefs(WorldMapSnapshot snapshot)
        {
            var list = snapshot.DefenseEvents ?? new List<DefenseEvent>();
            UnityEngine.PlayerPrefs.SetInt(_keyPrefix + ".defevt.count", list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                var e = list[i];
                var p = _keyPrefix + ".defevt." + i;
                UnityEngine.PlayerPrefs.SetString(p + ".id", e.DefenseEventId);
                UnityEngine.PlayerPrefs.SetString(p + ".battle", e.BattleId);
                UnityEngine.PlayerPrefs.SetString(p + ".report", e.ReportId);
                UnityEngine.PlayerPrefs.SetString(
                    p + ".at",
                    e.ScheduledAtUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetInt(p + ".status", (int)e.Status);
                UnityEngine.PlayerPrefs.SetInt(p + ".applied", e.ResultApplied ? 1 : 0);
                UnityEngine.PlayerPrefs.SetString(p + ".atk.id", e.Attacker.AttackerId);
                UnityEngine.PlayerPrefs.SetString(p + ".atk.name", e.Attacker.DisplayName);
                UnityEngine.PlayerPrefs.SetInt(p + ".atk.power", e.Attacker.Power);
                UnityEngine.PlayerPrefs.SetInt(p + ".atk.troops", e.Attacker.TroopCount);
            }
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
