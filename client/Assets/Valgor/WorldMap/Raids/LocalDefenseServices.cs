using System;
using System.Collections.Generic;
using System.Linq;
using Valgor.City.Buildings;
using Valgor.City.Data;
using Valgor.Dragons.Data;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Agenda e processa defesa local NPC offline (idempotente).</summary>
    public sealed class LocalDefenseScheduler
    {
        private readonly List<DefenseEvent> _events = new();
        private readonly ProcessedOperationRegistry _processed;

        public LocalDefenseScheduler(ProcessedOperationRegistry processed)
        {
            _processed = processed ?? throw new ArgumentNullException(nameof(processed));
        }

        public IReadOnlyList<DefenseEvent> Events => _events;

        public DefenseEvent Schedule(
            string defenseEventId,
            string battleId,
            DateTime scheduledAtUtc,
            NpcAttackerSnapshot attacker)
        {
            if (string.IsNullOrWhiteSpace(defenseEventId) || string.IsNullOrWhiteSpace(battleId))
            {
                throw new ArgumentException("DefenseEventId/BattleId obrigatórios.");
            }

            var existing = _events.FirstOrDefault(e =>
                string.Equals(e.DefenseEventId, defenseEventId, StringComparison.Ordinal));
            if (existing != null)
            {
                return existing;
            }

            var evt = new DefenseEvent
            {
                DefenseEventId = defenseEventId,
                BattleId = battleId,
                ReportId = battleId + ":defense-report",
                ScheduledAtUtc = scheduledAtUtc,
                Status = DefenseEventStatus.Pending,
                Attacker = attacker ?? new NpcAttackerSnapshot(),
                ResultApplied = false
            };
            _events.Add(evt);
            return evt;
        }

        public void ReplaceAll(IEnumerable<DefenseEvent> events)
        {
            _events.Clear();
            if (events == null)
            {
                return;
            }

            foreach (var e in events)
            {
                _events.Add(e);
            }
        }

        public IReadOnlyList<DefenseEvent> CloneEvents()
        {
            var list = new List<DefenseEvent>(_events.Count);
            foreach (var e in _events)
            {
                list.Add(CloneEvent(e));
            }

            return list;
        }

        public static DefenseEvent CloneEvent(DefenseEvent e) =>
            new()
            {
                DefenseEventId = e.DefenseEventId,
                BattleId = e.BattleId,
                ReportId = e.ReportId,
                ScheduledAtUtc = e.ScheduledAtUtc,
                Status = e.Status,
                Attacker = new NpcAttackerSnapshot
                {
                    AttackerId = e.Attacker.AttackerId,
                    DisplayName = e.Attacker.DisplayName,
                    Power = e.Attacker.Power,
                    TroopCount = e.Attacker.TroopCount
                },
                ResultApplied = e.ResultApplied,
                Result = e.Result
            };
    }

    public static class LocalDefenseResolver
    {
        public static CityRaidBattleResult Resolve(
            DefenseEvent evt,
            int wallLevel,
            long wallMax,
            long wallCurrent,
            decimal wallBonus,
            decimal wallReduction,
            int hospitalFree,
            ResourceBundle cityResources,
            ResourceBundle warehouseProtection,
            ResourceBundle minimumReserve,
            DragonBattleSnapshot? guardDragon)
        {
            var dragon = guardDragon ?? new DragonBattleSnapshot
            {
                Role = DragonBattleRole.Support,
                SupportPower = 0
            };
            if (dragon.Role != DragonBattleRole.Guard)
            {
                dragon = new DragonBattleSnapshot
                {
                    DragonId = dragon.DragonId,
                    DragonLevel = dragon.DragonLevel,
                    Role = DragonBattleRole.Support,
                    SupportPower = 0,
                    Energy = dragon.Energy
                };
            }

            return CityRaidBattleResolver.Resolve(new CityRaidBattleInput
            {
                BattleId = evt.BattleId,
                IsCityRaid = false,
                IsDefense = true,
                Attacker = new FormationSnapshot
                {
                    Power = evt.Attacker.Power,
                    TroopCount = Math.Max(1, evt.Attacker.TroopCount),
                    Label = evt.Attacker.DisplayName
                },
                Defender = new FormationSnapshot
                {
                    Power = SupportBuildingRules.GetWallCityDefense(wallLevel) +
                            (dragon.Role == DragonBattleRole.Guard ? dragon.SupportPower : 0),
                    TroopCount = 1000,
                    Label = "Defesa da City"
                },
                Wall = new WallSnapshot
                {
                    WallLevel = wallLevel,
                    MaxDurability = wallMax,
                    CurrentDurability = wallCurrent,
                    DefenseBonus = wallBonus,
                    DamageReduction = wallReduction
                },
                Dragon = dragon,
                HospitalFreeCapacity = hospitalFree,
                DefenderResources = cityResources,
                WarehouseProtection = warehouseProtection,
                MinimumReserve = minimumReserve,
                MarchCapacity = 500,
                LootTransactionId = evt.BattleId + ":defense-loot"
            });
        }
    }

    public sealed class PendingDefenseProcessor
    {
        private readonly LocalDefenseScheduler _scheduler;
        private readonly ProcessedOperationRegistry _processed;

        public PendingDefenseProcessor(LocalDefenseScheduler scheduler, ProcessedOperationRegistry processed)
        {
            _scheduler = scheduler;
            _processed = processed;
        }

        public int ProcessDue(
            DateTime utcNow,
            Func<DefenseEvent, CityRaidBattleResult> resolve,
            Action<DefenseEvent, CityRaidBattleResult> apply,
            out string log)
        {
            var due = _scheduler.Events
                .Where(e => e.Status is DefenseEventStatus.Pending or DefenseEventStatus.Resolving)
                .Where(e => e.ScheduledAtUtc <= utcNow)
                .OrderBy(e => e.ScheduledAtUtc)
                .ThenBy(e => e.DefenseEventId, StringComparer.Ordinal)
                .ToList();

            var count = 0;
            var sb = new System.Text.StringBuilder();
            foreach (var evt in due)
            {
                if (_processed.DefenseEventIds.Contains(evt.DefenseEventId) ||
                    _processed.BattleIds.Contains(evt.BattleId))
                {
                    evt.Status = DefenseEventStatus.Completed;
                    evt.ResultApplied = true;
                    sb.AppendLine($"skip-idempotent {evt.DefenseEventId}");
                    continue;
                }

                if (evt.Status == DefenseEventStatus.Resolving && evt.ResultApplied)
                {
                    evt.Status = DefenseEventStatus.Completed;
                    _processed.DefenseEventIds.Add(evt.DefenseEventId);
                    sb.AppendLine($"complete-interrupted {evt.DefenseEventId}");
                    count++;
                    continue;
                }

                evt.Status = DefenseEventStatus.Resolving;
                var result = resolve(evt);
                apply(evt, result);
                evt.Result = result;
                evt.ResultApplied = true;
                evt.Status = DefenseEventStatus.Completed;
                _processed.DefenseEventIds.Add(evt.DefenseEventId);
                _processed.BattleIds.Add(evt.BattleId);
                if (!string.IsNullOrWhiteSpace(evt.ReportId))
                {
                    _processed.ReportIds.Add(evt.ReportId);
                }

                sb.AppendLine($"resolved {evt.DefenseEventId} {result.Outcome}");
                count++;
            }

            log = sb.ToString();
            return count;
        }
    }
}
