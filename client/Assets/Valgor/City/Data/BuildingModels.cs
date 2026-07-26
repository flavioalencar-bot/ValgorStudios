using System;
using System.Collections.Generic;

namespace Valgor.City.Data
{
    public enum BuildingState
    {
        Locked,
        Available,
        Constructing,
        Ready,
        Upgrading
    }

    [Serializable]
    public sealed class BuildingDefinition
    {
        public BuildingDefinition(string id, string displayName, int maxLevel, IReadOnlyDictionary<ResourceType, long> baseCosts)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            MaxLevel = maxLevel;
            BaseCosts = baseCosts ?? throw new ArgumentNullException(nameof(baseCosts));
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int MaxLevel { get; }
        public IReadOnlyDictionary<ResourceType, long> BaseCosts { get; }

        public long GetUpgradeCost(ResourceType resource, int currentLevel)
        {
            return BaseCosts.TryGetValue(resource, out var baseCost) ? baseCost * Math.Max(1, currentLevel + 1) : 0;
        }
    }

    [Serializable]
    public sealed class BuildingInstance
    {
        public BuildingInstance(string definitionId, int level, BuildingState state)
        {
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            Level = level;
            State = state;
        }

        public string DefinitionId { get; }
        public int Level { get; private set; }
        public BuildingState State { get; private set; }

        public bool CanUpgrade(BuildingDefinition definition) =>
            (State == BuildingState.Ready || State == BuildingState.Available) && Level < definition.MaxLevel;

        public void CompleteUpgrade()
        {
            Level++;
            State = BuildingState.Ready;
        }
    }
}
