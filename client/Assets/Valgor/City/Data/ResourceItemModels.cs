using System;

namespace Valgor.City.Data
{
    public enum ResourceItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Premium = 4
    }

    /// <summary>Item de inventário que concede recurso (pacotes/baús). Sem compra real.</summary>
    public sealed class ResourceItemDefinition
    {
        public ResourceItemDefinition(
            string itemId,
            string displayName,
            ResourceType resourceId,
            long value,
            ResourceItemRarity rarity,
            int usagePriority,
            bool selectableResource = false)
        {
            ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ResourceId = resourceId;
            Value = value;
            Rarity = rarity;
            UsagePriority = usagePriority;
            SelectableResource = selectableResource;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public ResourceType ResourceId { get; }
        public long Value { get; }
        public ResourceItemRarity Rarity { get; }
        public int Quantity { get; set; }
        public int UsagePriority { get; }
        public bool SelectableResource { get; }
    }

    public sealed class ResourceItemStack
    {
        public ResourceItemStack(ResourceItemDefinition definition, int quantity)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Quantity = Math.Max(0, quantity);
        }

        public ResourceItemDefinition Definition { get; }
        public int Quantity { get; set; }
        public string ItemId => Definition.ItemId;
        public ResourceType ResourceId => Definition.ResourceId;
        public long Value => Definition.Value;
        public int UsagePriority => Definition.UsagePriority;
    }

    public sealed class AutoRefillPlanLine
    {
        public AutoRefillPlanLine(string itemId, string displayName, int quantity, long valueEach, long totalValue)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Quantity = quantity;
            ValueEach = valueEach;
            TotalValue = totalValue;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public int Quantity { get; }
        public long ValueEach { get; }
        public long TotalValue { get; }
    }

    public sealed class AutoRefillPlan
    {
        public ResourceType ResourceId { get; set; }
        public long BeforeAmount { get; set; }
        public long AfterAmount { get; set; }
        public long RequiredAmount { get; set; }
        public long TotalObtained { get; set; }
        public bool CompletesRequirement { get; set; }
        public AutoRefillPlanLine[] Lines { get; set; } = Array.Empty<AutoRefillPlanLine>();
    }
}
