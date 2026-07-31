using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.WorldMap.Raids
{
    public sealed class RaidApplicationRequest
    {
        public string RaidId { get; set; } = string.Empty;
        public string MarchId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public CityRaidBattleResult Result { get; set; } = new();
        public ResourceWallet? AttackerWallet { get; set; }
        public Action? Persist { get; set; }
    }

    public sealed class RaidApplicationOutcome
    {
        public bool AlreadyApplied { get; set; }
        public bool Applied { get; set; }
        public string Message { get; set; } = string.Empty;
        public CityBattleReport? Report { get; set; }
    }

    /// <summary>Porta única de aplicação idempotente do resultado de raid.</summary>
    public sealed class RaidResultApplicationService
    {
        private readonly ProcessedOperationRegistry _processed;

        public RaidResultApplicationService(ProcessedOperationRegistry processed)
        {
            _processed = processed ?? throw new ArgumentNullException(nameof(processed));
        }

        public RaidApplicationOutcome Apply(RaidApplicationRequest request)
        {
            if (request?.Result == null || string.IsNullOrWhiteSpace(request.Result.BattleId))
            {
                return new RaidApplicationOutcome { Message = "Resultado inválido." };
            }

            var battleId = request.Result.BattleId;
            if (_processed.BattleIds.Contains(battleId))
            {
                return new RaidApplicationOutcome
                {
                    AlreadyApplied = true,
                    Message = "BattleId já processado."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.RaidId) && _processed.RaidIds.Contains(request.RaidId))
            {
                return new RaidApplicationOutcome
                {
                    AlreadyApplied = true,
                    Message = "RaidId já processado."
                };
            }

            var lootId = request.Result.Loot?.LootTransactionId ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(lootId) && _processed.LootTransactionIds.Contains(lootId))
            {
                return new RaidApplicationOutcome
                {
                    AlreadyApplied = true,
                    Message = "LootTransactionId já processado."
                };
            }

            // Snapshot de rollback em memória.
            var walletSnap = SnapshotWallet(request.AttackerWallet);
            var processedSnap = _processed.Clone();

            try
            {
                if (request.Result.Outcome == BattleOutcome.Victory &&
                    request.AttackerWallet != null &&
                    request.Result.Loot != null)
                {
                    foreach (var pair in request.Result.Loot.LootedResources.AsDictionary())
                    {
                        request.AttackerWallet.Add(pair.Key, pair.Value);
                    }

                    RaidLootCalculator.MarkApplied(request.Result.Loot, _processed);
                }

                var reportId = string.IsNullOrWhiteSpace(request.ReportId)
                    ? battleId + ":report"
                    : request.ReportId;
                if (_processed.ReportIds.Contains(reportId))
                {
                    throw new InvalidOperationException("ReportId duplicado.");
                }

                var report = new CityBattleReport
                {
                    CreatedUtc = DateTime.UtcNow,
                    TargetCityId = request.RaidId,
                    Victory = request.Result.Outcome == BattleOutcome.Victory,
                    IsDefense = false,
                    AttackerPower = request.Result.AttackerPowerFinal,
                    DefensePower = request.Result.DefenderPowerFinal,
                    TroopWoundedPercent = request.Result.AttackerCasualties.TotalCasualties <= 0
                        ? 0
                        : (int)Math.Round(
                            100.0 * request.Result.AttackerCasualties.Wounded /
                            Math.Max(1, request.Result.AttackerCasualties.TotalCasualties)),
                    LootGranted = request.Result.Outcome == BattleOutcome.Victory &&
                                  (request.Result.Loot?.CapacityUsed ?? 0) > 0,
                    Summary = request.Result.Summary,
                    BattleId = battleId,
                    ReportId = reportId,
                    WallDurabilityAfter = request.Result.WallDurabilityAfter,
                    DominatorWrathUsed = request.Result.DominatorWrathUsed
                };

                _processed.BattleIds.Add(battleId);
                _processed.ReportIds.Add(reportId);
                if (!string.IsNullOrWhiteSpace(request.RaidId))
                {
                    _processed.RaidIds.Add(request.RaidId);
                }

                if (!string.IsNullOrWhiteSpace(request.MarchId))
                {
                    _processed.MarchIds.Add(request.MarchId);
                }

                request.Persist?.Invoke();
                return new RaidApplicationOutcome
                {
                    Applied = true,
                    Report = report,
                    Message = "Aplicado."
                };
            }
            catch (Exception ex)
            {
                RestoreWallet(request.AttackerWallet, walletSnap);
                RestoreProcessed(_processed, processedSnap);
                return new RaidApplicationOutcome
                {
                    Message = "Falha na aplicação: " + ex.Message
                };
            }
        }

        private static Dictionary<ResourceType, long> SnapshotWallet(ResourceWallet? wallet)
        {
            var map = new Dictionary<ResourceType, long>();
            if (wallet == null)
            {
                return map;
            }

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                map[type] = wallet.Get(type);
            }

            return map;
        }

        private static void RestoreWallet(ResourceWallet? wallet, Dictionary<ResourceType, long> snap)
        {
            if (wallet == null)
            {
                return;
            }

            foreach (var pair in snap)
            {
                var current = wallet.Get(pair.Key);
                var delta = pair.Value - current;
                if (delta > 0)
                {
                    wallet.Add(pair.Key, delta);
                }
                else if (delta < 0)
                {
                    wallet.TrySpend(pair.Key, -delta);
                }
            }
        }

        private static void RestoreProcessed(ProcessedOperationRegistry target, ProcessedOperationRegistry snap)
        {
            target.RaidIds.Clear();
            target.MarchIds.Clear();
            target.BattleIds.Clear();
            target.LootTransactionIds.Clear();
            target.ReportIds.Clear();
            target.WallRepairIds.Clear();
            target.DefenseEventIds.Clear();
            foreach (var id in snap.RaidIds) target.RaidIds.Add(id);
            foreach (var id in snap.MarchIds) target.MarchIds.Add(id);
            foreach (var id in snap.BattleIds) target.BattleIds.Add(id);
            foreach (var id in snap.LootTransactionIds) target.LootTransactionIds.Add(id);
            foreach (var id in snap.ReportIds) target.ReportIds.Add(id);
            foreach (var id in snap.WallRepairIds) target.WallRepairIds.Add(id);
            foreach (var id in snap.DefenseEventIds) target.DefenseEventIds.Add(id);
        }
    }
}
