using System;
using System.Collections.Generic;

namespace Valgor.Dragons.Data
{
    public enum DragonState
    {
        Locked = 0,
        Egg = 1,
        Hatching = 2,
        Juvenile = 3,
        Ready = 4,
        Deployed = 5,
        Hungry = 6,
        Exhausted = 7,
        Injured = 8,
        Recovering = 9,
        Resting = 10
    }

    /// <summary>
    /// Estágio de crescimento (eixo separado do DragonState operacional).
    /// </summary>
    public enum DragonGrowthStage
    {
        Egg = 0,
        Hatchling = 1,
        Juvenile = 2,
        Adult = 3,
        Elder = 4,
        Ancient = 5
    }

    public sealed class DragonChangedEvent : EventArgs
    {
        public DragonChangedEvent(string dragonId, DragonState previousState, DragonState currentState)
        {
            DragonId = dragonId;
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public string DragonId { get; }
        public DragonState PreviousState { get; }
        public DragonState CurrentState { get; }
    }

    public sealed class DragonSettings
    {
        public string PersistenceKey { get; set; } = "valgor.dragons.v3";
        public string DefaultRoostId { get; set; } = "dragon-tower";
        public int DefaultRoostCapacity { get; set; } = 3;
        public double HatchDurationHours { get; set; } = 1;
        public double JuvenileDurationHours { get; set; } = 2;
        public double RestDurationHours { get; set; } = 1;
        public double HungerIntervalHours { get; set; } = 6;
        public int HungerDecayPerTick { get; set; } = 10;
        public double HungryThresholdRatio { get; set; } = 0.25;
        public double ReadyHungerRatio { get; set; } = 0.5;
        public double RecoveryDurationHours { get; set; } = 2;
        public long FeedFoodCost { get; set; } = 200;
        public long FeedEssenceCost { get; set; } = 10;
        public int FeedHungerRestore { get; set; } = 40;
        public int GrowthPointsPerFeed { get; set; } = 8;
        public int GrowthPointsPerMission { get; set; } = 12;
        public int HatchlingToJuvenilePoints { get; set; } = 20;
        public int JuvenileToAdultPoints { get; set; } = 40;
        public int AdultToElderPoints { get; set; } = 100;
        public int ElderToAncientPoints { get; set; } = 200;
        public int BondPointsPerFeed { get; set; } = 5;
        public int BondPointsPerMission { get; set; } = 8;
        public int BondPointsPerLevel { get; set; } = 25;
        public int MaxBondLevel { get; set; } = 5;
        public int EvolutionMinBondLevel { get; set; } = 2;
        public DragonGrowthStage EvolutionMinGrowthStage { get; set; } = DragonGrowthStage.Adult;
    }

    public sealed class DragonDefinition
    {
        public DragonDefinition(
            string id,
            string displayName,
            string species,
            int tier,
            int basePower,
            int maxHunger,
            string worldNodeCode = "")
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Species = species ?? string.Empty;
            Tier = tier;
            BasePower = basePower;
            MaxHunger = maxHunger;
            WorldNodeCode = worldNodeCode ?? string.Empty;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Species { get; }
        public int Tier { get; }
        public int BasePower { get; }
        public int MaxHunger { get; }
        public string WorldNodeCode { get; }
    }

    public sealed class DragonInstance
    {
        public DragonInstance(
            string instanceId,
            string definitionId,
            DragonState state,
            int hunger,
            DateTime? stateEndsAtUtc = null,
            string? assignedMarchId = null,
            string? roostId = null)
        {
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            State = state;
            Hunger = hunger;
            StateEndsAtUtc = stateEndsAtUtc;
            AssignedMarchId = assignedMarchId;
            RoostId = roostId;
        }

        public string InstanceId { get; }
        public string DefinitionId { get; set; }
        public DragonState State { get; set; }
        public int Hunger { get; set; }
        public DateTime? StateEndsAtUtc { get; set; }
        public string? AssignedMarchId { get; set; }
        public string? RoostId { get; set; }
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        public DragonGrowthStage GrowthStage { get; set; } = DragonGrowthStage.Egg;
        public int GrowthPoints { get; set; }
        public int BondLevel { get; set; }
        public int BondPoints { get; set; }

        public DragonInstance Clone() =>
            new(InstanceId, DefinitionId, State, Hunger, StateEndsAtUtc, AssignedMarchId, RoostId)
            {
                LastUpdatedUtc = LastUpdatedUtc,
                GrowthStage = GrowthStage,
                GrowthPoints = GrowthPoints,
                BondLevel = BondLevel,
                BondPoints = BondPoints
            };
    }

    /// <summary>
    /// Ninho/torre onde dragões vivem (vinculado à Torre dos Dragões da cidade).
    /// </summary>
    public sealed class DragonRoost
    {
        public DragonRoost(string roostId, string buildingDefinitionId, int capacity, int level = 1)
        {
            RoostId = roostId ?? throw new ArgumentNullException(nameof(roostId));
            BuildingDefinitionId = buildingDefinitionId ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            Capacity = capacity;
            Level = level;
        }

        public string RoostId { get; }
        public string BuildingDefinitionId { get; }
        public int Capacity { get; set; }
        public int Level { get; set; }
        public List<string> OccupantIds { get; } = new();

        public bool HasSlot => OccupantIds.Count < Capacity;
    }

    public static class DragonCatalog
    {
        private static readonly Dictionary<string, DragonDefinition> Definitions = new()
        {
            ["ember-whelp"] = new("ember-whelp", "Filhote de Brasa", "Wyrmling", 1, 80, 100, "ash-drake"),
            ["ash-drake"] = new("ash-drake", "Drake de Cinzas", "Drake", 2, 160, 120, "ash-drake"),
            ["portal-wyrm"] = new("portal-wyrm", "Wyrm do Portal", "Wyrm", 3, 280, 150, "portal-wyrm"),
            ["storm-rider"] = new("storm-rider", "Cavaleiro da Tempestade", "Drake", 2, 200, 130)
        };

        public static IReadOnlyDictionary<string, DragonDefinition> All => Definitions;

        public static DragonDefinition Get(string id) => Definitions[id];

        public static bool TryGet(string id, out DragonDefinition definition) =>
            Definitions.TryGetValue(id, out definition!);

        public static bool TryGetByWorldCode(string worldNodeCode, out DragonDefinition definition)
        {
            foreach (var pair in Definitions)
            {
                if (string.Equals(pair.Value.WorldNodeCode, worldNodeCode, StringComparison.Ordinal))
                {
                    definition = pair.Value;
                    return true;
                }
            }

            definition = null!;
            return false;
        }
    }
}
