using System;
using Valgor.City.Data;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Saque determinístico (puro).</summary>
    public static class RaidLootCalculator
    {
        public static RaidLootResult Calculate(
            BattleOutcome outcome,
            ResourceBundle lootable,
            long marchCapacity,
            string lootTransactionId,
            ProcessedOperationRegistry? processed = null)
        {
            var result = new RaidLootResult
            {
                LootTransactionId = lootTransactionId ?? string.Empty,
                MarchCapacity = Math.Max(0, marchCapacity),
                VictoryRequired = true,
                LootedResources = new ResourceBundle()
            };

            if (string.IsNullOrWhiteSpace(result.LootTransactionId))
            {
                return result;
            }

            if (processed != null && processed.LootTransactionIds.Contains(result.LootTransactionId))
            {
                result.Applied = true;
                return result;
            }

            if (outcome != BattleOutcome.Victory)
            {
                return result;
            }

            lootable ??= new ResourceBundle();
            var remaining = result.MarchCapacity;
            foreach (var type in RaidCombatCatalog.LootOrder)
            {
                if (WarehouseProtectionResolver.IsNeverLootable(type))
                {
                    continue;
                }

                if (remaining <= 0)
                {
                    break;
                }

                var pool = lootable.Get(type);
                var limit = (long)Math.Floor(pool * RaidCombatCatalog.LootableFraction);
                if (limit <= 0)
                {
                    continue;
                }

                var take = Math.Min(limit, remaining);
                if (take <= 0)
                {
                    continue;
                }

                result.LootedResources.Set(type, take);
                remaining -= take;
            }

            result.CapacityUsed = result.MarchCapacity - remaining;
            return result;
        }

        public static void MarkApplied(RaidLootResult loot, ProcessedOperationRegistry processed)
        {
            if (loot == null || processed == null || string.IsNullOrWhiteSpace(loot.LootTransactionId))
            {
                return;
            }

            processed.LootTransactionIds.Add(loot.LootTransactionId);
            loot.Applied = true;
        }
    }
}
