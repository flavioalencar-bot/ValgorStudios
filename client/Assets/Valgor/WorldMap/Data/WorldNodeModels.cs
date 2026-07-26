using System;
using Valgor.City.Data;

namespace Valgor.WorldMap.Data
{
    public enum WorldNodeKind
    {
        City,
        Village,
        Resource,
        Creature,
        Dragon,
        Landmark
    }

    public enum WorldNodeStatus
    {
        Locked,
        Available,
        Occupied,
        Depleted,
        Respawning,
        Cleared
    }

    public abstract class WorldMapNodeDefinition
    {
        protected WorldMapNodeDefinition(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus defaultStatus,
            float x,
            float z)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            RegionId = regionId ?? throw new ArgumentNullException(nameof(regionId));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? string.Empty;
            DefaultStatus = defaultStatus;
            X = x;
            Z = z;
        }

        public string Id { get; }
        public string RegionId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public WorldNodeStatus DefaultStatus { get; }
        public float X { get; }
        public float Z { get; }
        public abstract WorldNodeKind Kind { get; }
    }

    public sealed class WorldCityNode : WorldMapNodeDefinition
    {
        public WorldCityNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            bool isPlayerHome = false)
            : base(id, regionId, displayName, description, status, x, z)
        {
            IsPlayerHome = isPlayerHome;
        }

        public bool IsPlayerHome { get; }
        public override WorldNodeKind Kind => WorldNodeKind.City;
    }

    public sealed class WorldVillageNode : WorldMapNodeDefinition
    {
        public WorldVillageNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            int population)
            : base(id, regionId, displayName, description, status, x, z)
        {
            Population = population;
        }

        public int Population { get; }
        public override WorldNodeKind Kind => WorldNodeKind.Village;
    }

    public sealed class WorldResourceNode : WorldMapNodeDefinition
    {
        public WorldResourceNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            ResourceType resourceType,
            long maxAmount,
            int level,
            double gatherRatePerHour,
            TimeSpan respawnDuration)
            : base(id, regionId, displayName, description, status, x, z)
        {
            if (resourceType == ResourceType.Diamonds)
            {
                throw new ArgumentException("Diamonds não possuem nó de coleta passiva no mapa.", nameof(resourceType));
            }

            if (maxAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAmount));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (gatherRatePerHour < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gatherRatePerHour));
            }

            if (respawnDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(respawnDuration));
            }

            ResourceType = resourceType;
            MaxAmount = maxAmount;
            Level = level;
            GatherRatePerHour = gatherRatePerHour;
            RespawnDuration = respawnDuration;
        }

        public ResourceType ResourceType { get; }
        public long MaxAmount { get; }
        public int Level { get; }
        public double GatherRatePerHour { get; }
        public TimeSpan RespawnDuration { get; }

        /// <summary>Alias legado usado por consumidores existentes.</summary>
        public ResourceType Resource => ResourceType;

        /// <summary>Alias legado de capacidade máxima.</summary>
        public long Amount => MaxAmount;

        public override WorldNodeKind Kind => WorldNodeKind.Resource;

        public double GetGatherRatePerHour() => GatherRatePerHour * Level;
    }

    public sealed class WorldCreatureNode : WorldMapNodeDefinition
    {
        public WorldCreatureNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            int threatLevel,
            string creatureCode)
            : base(id, regionId, displayName, description, status, x, z)
        {
            ThreatLevel = threatLevel;
            CreatureCode = creatureCode ?? throw new ArgumentNullException(nameof(creatureCode));
        }

        public int ThreatLevel { get; }
        public string CreatureCode { get; }
        public override WorldNodeKind Kind => WorldNodeKind.Creature;
    }

    public sealed class WorldDragonNode : WorldMapNodeDefinition
    {
        public WorldDragonNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            string dragonCode)
            : base(id, regionId, displayName, description, status, x, z)
        {
            DragonCode = dragonCode ?? throw new ArgumentNullException(nameof(dragonCode));
        }

        public string DragonCode { get; }
        public override WorldNodeKind Kind => WorldNodeKind.Dragon;
    }

    public sealed class WorldLandmarkNode : WorldMapNodeDefinition
    {
        public WorldLandmarkNode(
            string id,
            string regionId,
            string displayName,
            string description,
            WorldNodeStatus status,
            float x,
            float z,
            string landmarkCode)
            : base(id, regionId, displayName, description, status, x, z)
        {
            LandmarkCode = landmarkCode ?? throw new ArgumentNullException(nameof(landmarkCode));
        }

        public string LandmarkCode { get; }
        public override WorldNodeKind Kind => WorldNodeKind.Landmark;
    }

    public sealed class WorldNodeInstance
    {
        public WorldNodeInstance(string definitionId, WorldNodeStatus status, long remainingAmount = 0)
        {
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            Status = status;
            RemainingAmount = remainingAmount < 0 ? 0 : remainingAmount;
            ResourceState = MapResourceState(status);
        }

        public string DefinitionId { get; }
        public WorldNodeStatus Status { get; set; }
        public long RemainingAmount { get; set; }
        public string? OccupiedByMarchId { get; set; }
        public DateTime? RespawnAt { get; set; }
        public DateTime? LastGatherUpdatedUtc { get; set; }
        public ResourceNodeState ResourceState { get; set; }

        public void SetResourceState(ResourceNodeState state)
        {
            ResourceState = state;
            Status = state switch
            {
                ResourceNodeState.Available => WorldNodeStatus.Available,
                ResourceNodeState.Occupied => WorldNodeStatus.Occupied,
                ResourceNodeState.Depleted => WorldNodeStatus.Depleted,
                ResourceNodeState.Respawning => WorldNodeStatus.Respawning,
                _ => Status
            };
        }

        private static ResourceNodeState MapResourceState(WorldNodeStatus status) => status switch
        {
            WorldNodeStatus.Occupied => ResourceNodeState.Occupied,
            WorldNodeStatus.Depleted => ResourceNodeState.Depleted,
            WorldNodeStatus.Respawning => ResourceNodeState.Respawning,
            _ => ResourceNodeState.Available
        };
    }
}
