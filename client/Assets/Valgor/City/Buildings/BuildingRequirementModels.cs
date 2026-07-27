using System;
using System.Collections.Generic;

namespace Valgor.City.Buildings
{
    /// <summary>Definição de upgrade data-driven por edifício (regras padrão + por nível-alvo).</summary>
    public sealed class BuildingUpgradeDefinition
    {
        public const int DynamicCastleLevel = -1;

        public BuildingUpgradeDefinition(
            string buildingDefinitionId,
            BuildingUpgradeRequirement? defaultRequirement = null,
            IReadOnlyDictionary<int, BuildingUpgradeRequirement>? byTargetLevel = null)
        {
            BuildingDefinitionId = buildingDefinitionId ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            DefaultRequirement = defaultRequirement;
            ByTargetLevel = byTargetLevel ?? new Dictionary<int, BuildingUpgradeRequirement>();
        }

        public string BuildingDefinitionId { get; }
        public BuildingUpgradeRequirement? DefaultRequirement { get; }
        public IReadOnlyDictionary<int, BuildingUpgradeRequirement> ByTargetLevel { get; }

        public BuildingUpgradeRequirement Resolve(int currentLevel)
        {
            var targetLevel = currentLevel + 1;
            if (ByTargetLevel.TryGetValue(targetLevel, out var specific))
            {
                return Normalize(specific, targetLevel);
            }

            if (DefaultRequirement != null)
            {
                return Normalize(DefaultRequirement, targetLevel);
            }

            return new BuildingUpgradeRequirement(minimumCastleLevel: targetLevel);
        }

        private static BuildingUpgradeRequirement Normalize(BuildingUpgradeRequirement requirement, int targetLevel)
        {
            var castle = requirement.MinimumCastleLevel == DynamicCastleLevel
                ? targetLevel
                : requirement.MinimumCastleLevel;
            return new BuildingUpgradeRequirement(
                castle,
                requirement.RequiredBuildings,
                requirement.RequiredUnlocks);
        }
    }

    /// <summary>Exige outro edifício em nível mínimo.</summary>
    public readonly struct BuildingLevelRequirement
    {
        public BuildingLevelRequirement(string buildingDefinitionId, int minimumLevel, string? label = null)
        {
            BuildingDefinitionId = buildingDefinitionId ?? throw new ArgumentNullException(nameof(buildingDefinitionId));
            MinimumLevel = Math.Max(0, minimumLevel);
            Label = label;
        }

        public string BuildingDefinitionId { get; }
        public int MinimumLevel { get; }
        public string? Label { get; }
    }

    /// <summary>Exige pesquisa / desbloqueio (ex.: Coleta +).</summary>
    public readonly struct BuildingUnlockRequirement
    {
        public BuildingUnlockRequirement(string unlockKey, string displayName)
        {
            UnlockKey = unlockKey ?? throw new ArgumentNullException(nameof(unlockKey));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }

        public string UnlockKey { get; }
        public string DisplayName { get; }
    }

    /// <summary>
    /// Pré-requisitos para evoluir um edifício do nível atual → atual+1
    /// (recursos e construtor ficam fora — validados pelo CityController).
    /// </summary>
    public sealed class BuildingUpgradeRequirement
    {
        public BuildingUpgradeRequirement(
            int minimumCastleLevel,
            IReadOnlyList<BuildingLevelRequirement>? requiredBuildings = null,
            IReadOnlyList<BuildingUnlockRequirement>? requiredUnlocks = null)
        {
            MinimumCastleLevel = minimumCastleLevel;
            RequiredBuildings = requiredBuildings ?? Array.Empty<BuildingLevelRequirement>();
            RequiredUnlocks = requiredUnlocks ?? Array.Empty<BuildingUnlockRequirement>();
        }

        public int MinimumCastleLevel { get; }
        public IReadOnlyList<BuildingLevelRequirement> RequiredBuildings { get; }
        public IReadOnlyList<BuildingUnlockRequirement> RequiredUnlocks { get; }
    }
}
