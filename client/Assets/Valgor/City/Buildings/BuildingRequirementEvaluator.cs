using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    public readonly struct BuildingDependencyCheck
    {
        public BuildingDependencyCheck(
            string label,
            bool satisfied,
            string detail,
            string? jumpToDefinitionId = null,
            int requiredMinimumLevel = 0)
        {
            Label = label ?? string.Empty;
            Satisfied = satisfied;
            Detail = detail ?? string.Empty;
            JumpToDefinitionId = jumpToDefinitionId;
            RequiredMinimumLevel = Math.Max(0, requiredMinimumLevel);
        }

        public string Label { get; }
        public bool Satisfied { get; }
        public string Detail { get; }

        /// <summary>Id do edifício para o botão Ir (null = sem navegação).</summary>
        public string? JumpToDefinitionId { get; }

        /// <summary>Nível mínimo exigido do edifício alvo (0 = N/A).</summary>
        public int RequiredMinimumLevel { get; }
    }

    /// <summary>Avalia pré-requisitos do catálogo (castelo / prédios / unlocks).</summary>
    public static class BuildingRequirementEvaluator
    {
        public static IReadOnlyList<BuildingDependencyCheck> Evaluate(
            BuildingInstance building,
            int castleLevel,
            Func<string, int> getBuildingLevel,
            Func<string, bool>? hasUnlock = null)
        {
            if (building == null) throw new ArgumentNullException(nameof(building));
            if (getBuildingLevel == null) throw new ArgumentNullException(nameof(getBuildingLevel));

            hasUnlock ??= static _ => false;

            var requirement = BuildingRequirementCatalog.GetRequirement(building.DefinitionId, building.Level);
            var list = new List<BuildingDependencyCheck>(4 + requirement.RequiredBuildings.Count);

            if (requirement.MinimumCastleLevel > 0)
            {
                var ok = castleLevel >= requirement.MinimumCastleLevel;
                list.Add(new BuildingDependencyCheck(
                    "Castelo",
                    ok,
                    ok
                        ? $"Nv.{castleLevel} ≥ {requirement.MinimumCastleLevel}"
                        : $"Requer Castelo Nv.{requirement.MinimumCastleLevel} (atual {castleLevel})",
                    jumpToDefinitionId: "castle",
                    requiredMinimumLevel: requirement.MinimumCastleLevel));
            }

            foreach (var dep in requirement.RequiredBuildings)
            {
                var level = getBuildingLevel(dep.BuildingDefinitionId);
                var ok = level >= dep.MinimumLevel;
                var name = string.IsNullOrEmpty(dep.Label) ? dep.BuildingDefinitionId : dep.Label!;
                list.Add(new BuildingDependencyCheck(
                    name,
                    ok,
                    ok
                        ? $"Nv.{level} ≥ {dep.MinimumLevel}"
                        : $"Requer {name} Nv.{dep.MinimumLevel} (atual {level})",
                    jumpToDefinitionId: dep.BuildingDefinitionId,
                    requiredMinimumLevel: dep.MinimumLevel));
            }

            foreach (var unlock in requirement.RequiredUnlocks)
            {
                var ok = hasUnlock(unlock.UnlockKey);
                list.Add(new BuildingDependencyCheck(
                    unlock.DisplayName,
                    ok,
                    ok ? "Desbloqueado" : $"Requer {unlock.DisplayName}"));
            }

            return list;
        }

        public static string? GetFirstBlockReason(
            BuildingInstance building,
            int castleLevel,
            Func<string, int> getBuildingLevel,
            Func<string, bool>? hasUnlock = null)
        {
            foreach (var check in Evaluate(building, castleLevel, getBuildingLevel, hasUnlock))
            {
                if (!check.Satisfied)
                {
                    return check.Detail;
                }
            }

            return null;
        }

        public static bool MeetsAll(
            BuildingInstance building,
            int castleLevel,
            Func<string, int> getBuildingLevel,
            Func<string, bool>? hasUnlock = null)
        {
            return GetFirstBlockReason(building, castleLevel, getBuildingLevel, hasUnlock) == null;
        }
    }
}
