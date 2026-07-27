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

        /// <summary>Duração beta curta (estilo fila LZ, sem timers de horas).</summary>
        public TimeSpan GetUpgradeDuration(int currentLevel) =>
            TimeSpan.FromSeconds(6 + Math.Max(0, currentLevel) * 4);
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
        public DateTime? UpgradeCompletesAtUtc { get; private set; }

        public bool CanUpgrade(BuildingDefinition definition) =>
            (State == BuildingState.Ready || State == BuildingState.Available) && Level < definition.MaxLevel;

        public void BeginUpgrade(DateTime completesAtUtc)
        {
            State = BuildingState.Upgrading;
            UpgradeCompletesAtUtc = DateTime.SpecifyKind(completesAtUtc, DateTimeKind.Utc);
        }

        public void CompleteUpgrade()
        {
            Level++;
            State = BuildingState.Ready;
            UpgradeCompletesAtUtc = null;
        }

        public void ApplyPersisted(int level, BuildingState state, DateTime? upgradeCompletesAtUtc)
        {
            Level = Math.Max(0, level);
            State = state;
            UpgradeCompletesAtUtc = upgradeCompletesAtUtc.HasValue
                ? DateTime.SpecifyKind(upgradeCompletesAtUtc.Value, DateTimeKind.Utc)
                : null;
        }
    }

    [Serializable]
    public sealed class BuildingProgressRecord
    {
        public string DefinitionId { get; set; } = string.Empty;
        public int Level { get; set; }
        public BuildingState State { get; set; }
        public DateTime? UpgradeCompletesAtUtc { get; set; }
    }
}
