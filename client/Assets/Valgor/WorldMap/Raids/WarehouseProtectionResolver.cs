using System;
using Valgor.City.Data;

namespace Valgor.WorldMap.Raids
{
    /// <summary>Proteção do Armazém e reserva mínima (puro).</summary>
    public static class WarehouseProtectionResolver
    {
        public static WarehouseProtectionResult Resolve(
            ResourceBundle current,
            ResourceBundle protectedResources,
            ResourceBundle minimumReserve)
        {
            current ??= new ResourceBundle();
            protectedResources ??= new ResourceBundle();
            minimumReserve ??= new ResourceBundle();

            var lootable = new ResourceBundle();
            foreach (var type in RaidCombatCatalog.LootOrder)
            {
                var cur = current.Get(type);
                var prot = protectedResources.Get(type);
                var reserve = minimumReserve.Get(type);
                var amount = Math.Max(cur - prot - reserve, 0);
                if (amount > 0)
                {
                    lootable.Set(type, amount);
                }
            }

            return new WarehouseProtectionResult
            {
                CurrentResources = current.Clone(),
                ProtectedResources = protectedResources.Clone(),
                MinimumReserve = minimumReserve.Clone(),
                LootableResources = lootable
            };
        }

        public static bool IsNeverLootable(ResourceType type) =>
            RaidCombatCatalog.NeverLootable.Contains(type) ||
            type is ResourceType.Diamonds;
    }
}
