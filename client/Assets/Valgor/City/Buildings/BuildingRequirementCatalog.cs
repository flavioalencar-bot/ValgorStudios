using System;
using System.Collections.Generic;

namespace Valgor.City.Buildings
{
    /// <summary>
    /// Catálogo data-driven de dependências de evolução (sem novos edifícios).
    /// Recursos e fila de construtor continuam no CityController.
    /// </summary>
    public static class BuildingRequirementCatalog
    {
        public const string UnlockGatherResearch = "research.gatherBoost";

        private static readonly IReadOnlyDictionary<string, BuildingUpgradeDefinition> Definitions =
            BuildCatalog();

        public static BuildingUpgradeRequirement GetRequirement(string buildingDefinitionId, int currentLevel)
        {
            if (string.IsNullOrWhiteSpace(buildingDefinitionId))
            {
                throw new ArgumentException("Id inválido.", nameof(buildingDefinitionId));
            }

            if (Definitions.TryGetValue(buildingDefinitionId, out var definition))
            {
                return definition.Resolve(currentLevel);
            }

            // Fallback: Castelo ≥ nível-alvo (sem lógica na UI).
            return new BuildingUpgradeRequirement(minimumCastleLevel: currentLevel + 1);
        }

        public static bool TryGetDefinition(string buildingDefinitionId, out BuildingUpgradeDefinition definition) =>
            Definitions.TryGetValue(buildingDefinitionId, out definition!);

        private static IReadOnlyDictionary<string, BuildingUpgradeDefinition> BuildCatalog()
        {
            var map = new Dictionary<string, BuildingUpgradeDefinition>(StringComparer.Ordinal)
            {
                // Castelo → Nv.N: Fazenda e Armazém nos níveis do catálogo (sem gate de Castelo).
                ["castle"] = new BuildingUpgradeDefinition(
                    "castle",
                    defaultRequirement: new BuildingUpgradeRequirement(minimumCastleLevel: 0),
                    byTargetLevel: new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = BuildingsOnly(
                            new BuildingLevelRequirement("farm", 1, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 1, "Armazém")),
                        [3] = BuildingsOnly(
                            new BuildingLevelRequirement("farm", 2, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 2, "Armazém")),
                        [4] = BuildingsOnly(
                            new BuildingLevelRequirement("farm", 3, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 3, "Armazém")),
                        [5] = BuildingsOnly(
                            new BuildingLevelRequirement("farm", 4, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 4, "Armazém")),
                        [6] = BuildingsOnly(
                            new BuildingLevelRequirement("farm", 5, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 5, "Armazém"))
                    }),

                // Fazenda → Nv.N: Castelo ≥ N.
                ["farm"] = new BuildingUpgradeDefinition("farm", DynamicCastle()),

                // Armazém → Nv.N: Castelo ≥ N; em níveis configurados, Fazenda no nível exigido.
                ["warehouse"] = new BuildingUpgradeDefinition(
                    "warehouse",
                    defaultRequirement: DynamicCastle(),
                    byTargetLevel: new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("farm", 2, "Fazenda")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("farm", 2, "Fazenda")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("farm", 2, "Fazenda")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("farm", 3, "Fazenda"))
                    }),

                ["lumbermill"] = new BuildingUpgradeDefinition("lumbermill", DynamicCastle()),
                ["quarry"] = new BuildingUpgradeDefinition("quarry", DynamicCastle()),

                ["mine"] = new BuildingUpgradeDefinition(
                    "mine",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [3] = WithBuildings(3, new BuildingLevelRequirement("quarry", 1, "Pedreira")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("quarry", 2, "Pedreira"))
                    }),

                ["academy"] = new BuildingUpgradeDefinition("academy", DynamicCastle()),

                ["institute"] = new BuildingUpgradeDefinition(
                    "institute",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia"))
                    }),

                ["hospital"] = new BuildingUpgradeDefinition("hospital", DynamicCastle()),

                ["market"] = new BuildingUpgradeDefinition(
                    "market",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("warehouse", 1, "Armazém"))
                    }),

                ["temple"] = new BuildingUpgradeDefinition("temple", DynamicCastle()),

                ["arena"] = new BuildingUpgradeDefinition(
                    "arena",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia"))
                    }),

                ["laboratory"] = new BuildingUpgradeDefinition(
                    "laboratory",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia"))
                    }),

                ["dragon-tower"] = new BuildingUpgradeDefinition(
                    "dragon-tower",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = new BuildingUpgradeRequirement(
                            minimumCastleLevel: 2,
                            requiredBuildings: new[]
                            {
                                new BuildingLevelRequirement("warehouse", 1, "Armazém")
                            },
                            requiredUnlocks: new[]
                            {
                                new BuildingUnlockRequirement(UnlockGatherResearch, "Pesquisa: Coleta +")
                            }),
                        [3] = WithBuildings(
                            3,
                            new BuildingLevelRequirement("warehouse", 2, "Armazém"),
                            new BuildingLevelRequirement("temple", 1, "Templo"))
                    })
            };

            return map;
        }

        private static BuildingUpgradeRequirement DynamicCastle() =>
            new(minimumCastleLevel: BuildingUpgradeDefinition.DynamicCastleLevel);

        private static BuildingUpgradeRequirement WithBuildings(
            int castleLevel,
            params BuildingLevelRequirement[] buildings) =>
            new(minimumCastleLevel: castleLevel, requiredBuildings: buildings);

        private static BuildingUpgradeRequirement BuildingsOnly(params BuildingLevelRequirement[] buildings) =>
            new(minimumCastleLevel: 0, requiredBuildings: buildings);
    }
}
