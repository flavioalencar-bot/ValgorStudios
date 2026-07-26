using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.WorldMap.Creatures
{
    public enum WorldCreatureState
    {
        Available,
        Engaged,
        Defeated,
        Respawning
    }

    public enum WorldCreatureType
    {
        Beast,
        Elemental,
        Construct,
        Undead,
        Dragonkin,
        Aberration
    }

    public enum CreatureDifficultyBand
    {
        Trivial,
        Easy,
        Fair,
        Hard,
        Impossible
    }

    public sealed class CreatureRewardEntry
    {
        public CreatureRewardEntry(ResourceType resource, long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Resource = resource;
            Amount = amount;
        }

        public ResourceType Resource { get; }
        public long Amount { get; }
    }

    public sealed class CreatureRewardTable
    {
        private readonly List<CreatureRewardEntry> _entries;

        public CreatureRewardTable(string id, IEnumerable<CreatureRewardEntry> entries)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _entries = new List<CreatureRewardEntry>(entries ?? throw new ArgumentNullException(nameof(entries)));
        }

        public string Id { get; }
        public IReadOnlyList<CreatureRewardEntry> Entries => _entries;

        public void GrantTo(ResourceWallet wallet)
        {
            foreach (var entry in _entries)
            {
                wallet.Add(entry.Resource, entry.Amount);
            }
        }
    }

    public sealed class WorldCreatureDefinition
    {
        public WorldCreatureDefinition(
            string id,
            WorldCreatureType type,
            string displayName,
            int level,
            int recommendedPower,
            int energyCost,
            TimeSpan respawnDuration,
            string regionId,
            float x,
            float z,
            CreatureRewardTable rewards,
            bool startsLocked = false)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (recommendedPower < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(recommendedPower));
            }

            if (energyCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(energyCost));
            }

            if (respawnDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(respawnDuration));
            }

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = type;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Level = level;
            RecommendedPower = recommendedPower;
            EnergyCost = energyCost;
            RespawnDuration = respawnDuration;
            RegionId = regionId ?? throw new ArgumentNullException(nameof(regionId));
            X = x;
            Z = z;
            Rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            StartsLocked = startsLocked;
        }

        public string Id { get; }
        public WorldCreatureType Type { get; }
        public string DisplayName { get; }
        public int Level { get; }
        public int RecommendedPower { get; }
        public int EnergyCost { get; }
        public TimeSpan RespawnDuration { get; }
        public string RegionId { get; }
        public float X { get; }
        public float Z { get; }
        public CreatureRewardTable Rewards { get; }
        public bool StartsLocked { get; }
    }

    public sealed class WorldCreatureInstance
    {
        public WorldCreatureInstance(
            string definitionId,
            WorldCreatureState state,
            string regionId,
            float x,
            float z,
            DateTime? respawnAtUtc = null)
        {
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            State = state;
            RegionId = regionId ?? throw new ArgumentNullException(nameof(regionId));
            X = x;
            Z = z;
            RespawnAtUtc = respawnAtUtc;
        }

        public string DefinitionId { get; }
        public WorldCreatureState State { get; set; }
        public string RegionId { get; }
        public float X { get; }
        public float Z { get; }
        public DateTime? RespawnAtUtc { get; set; }
        public string? EngagedMarchId { get; set; }
    }
}
