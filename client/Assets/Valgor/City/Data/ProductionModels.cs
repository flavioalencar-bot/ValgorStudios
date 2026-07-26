using System;
using System.Collections.Generic;

namespace Valgor.City.Data
{
    /// <summary>
    /// Configuração global de produção (valores centralizados, sem magic numbers espalhados).
    /// </summary>
    public sealed class ProductionSettings
    {
        public TimeSpan MaxOfflineDuration { get; set; } = TimeSpan.FromHours(12);
        public TimeSpan TickInterval { get; set; } = TimeSpan.FromSeconds(1);
        public string PersistenceKey { get; set; } = "valgor.city.production.v1";
    }

    public sealed class ResourceProductionDefinition
    {
        public ResourceProductionDefinition(
            string buildingDefinitionId,
            ResourceType resource,
            double baseRatePerHour,
            long baseCapacity)
        {
            if (resource == ResourceType.Diamonds)
            {
                throw new ArgumentException("Diamonds cannot have passive building production.", nameof(resource));
            }

            if (baseRatePerHour < 0 || baseCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseRatePerHour));
            }

            BuildingDefinitionId = buildingDefinitionId ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            Resource = resource;
            BaseRatePerHour = baseRatePerHour;
            BaseCapacity = baseCapacity;
        }

        public string BuildingDefinitionId { get; }
        public ResourceType Resource { get; }
        public double BaseRatePerHour { get; }
        public long BaseCapacity { get; }

        public double GetRatePerHour(int level) => level <= 0 ? 0 : BaseRatePerHour * level;

        public long GetCapacity(int level) => level <= 0 ? 0 : checked(BaseCapacity * level);
    }

    public sealed class BuildingProductionState
    {
        public BuildingProductionState(string buildingDefinitionId, DateTime lastUpdatedUtc)
        {
            BuildingDefinitionId = buildingDefinitionId ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            LastUpdatedUtc = lastUpdatedUtc;
        }

        public string BuildingDefinitionId { get; }
        public long Accumulated { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public bool HasCollectable => Accumulated > 0;
    }

    public sealed class ProductionChangedEvent : EventArgs
    {
        public ProductionChangedEvent(string buildingDefinitionId, ResourceType resource, long accumulated, long capacity)
        {
            BuildingDefinitionId = buildingDefinitionId;
            Resource = resource;
            Accumulated = accumulated;
            Capacity = capacity;
        }

        public string BuildingDefinitionId { get; }
        public ResourceType Resource { get; }
        public long Accumulated { get; }
        public long Capacity { get; }
    }

    /// <summary>
    /// Catálogo configurável de produção passiva. Diamonds não entram aqui.
    /// </summary>
    public static class ProductionCatalog
    {
        public static ProductionSettings Settings { get; } = new();

        private static readonly Dictionary<string, ResourceProductionDefinition> Definitions = new()
        {
            ["farm"] = new("farm", ResourceType.Food, baseRatePerHour: 120, baseCapacity: 500),
            ["lumbermill"] = new("lumbermill", ResourceType.Wood, baseRatePerHour: 100, baseCapacity: 400),
            ["quarry"] = new("quarry", ResourceType.Stone, baseRatePerHour: 80, baseCapacity: 400),
            ["mine"] = new("mine", ResourceType.Iron, baseRatePerHour: 60, baseCapacity: 300),
            ["market"] = new("market", ResourceType.Gold, baseRatePerHour: 90, baseCapacity: 600),
            ["dragon-tower"] = new("dragon-tower", ResourceType.DragonEssence, baseRatePerHour: 10, baseCapacity: 50)
        };

        public static bool TryGet(string buildingDefinitionId, out ResourceProductionDefinition definition) =>
            Definitions.TryGetValue(buildingDefinitionId, out definition!);

        public static IReadOnlyDictionary<string, ResourceProductionDefinition> All => Definitions;
    }
}
