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
                            new BuildingLevelRequirement("farm", 2, "Fazenda"),
                            new BuildingLevelRequirement("warehouse", 2, "Armazém")),
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

                // Serraria → Nv.N: Castelo ≥ N; Fazenda em níveis configurados.
                ["lumbermill"] = new BuildingUpgradeDefinition(
                    "lumbermill",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("farm", 1, "Fazenda")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("farm", 1, "Fazenda")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("farm", 2, "Fazenda")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("farm", 2, "Fazenda"))
                    }),

                // Pedreira → Nv.N: Castelo ≥ N; Serraria em níveis configurados.
                ["quarry"] = new BuildingUpgradeDefinition(
                    "quarry",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("lumbermill", 1, "Serraria")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("lumbermill", 2, "Serraria")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("lumbermill", 2, "Serraria")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("lumbermill", 3, "Serraria"))
                    }),

                ["mine"] = new BuildingUpgradeDefinition(
                    "mine",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("quarry", 1, "Pedreira")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("quarry", 1, "Pedreira")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("quarry", 2, "Pedreira")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("quarry", 2, "Pedreira"))
                    }),

                // Academia → Nv.N: Castelo ≥ N; Armazém em níveis configurados.
                ["academy"] = new BuildingUpgradeDefinition(
                    "academy",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("warehouse", 1, "Armazém")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("warehouse", 1, "Armazém")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("warehouse", 2, "Armazém")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("warehouse", 2, "Armazém"))
                    }),

                ["institute"] = new BuildingUpgradeDefinition(
                    "institute",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia"))
                    }),

                // Hospital → Castelo ≥ N; Fazenda (início) / Armazém (níveis altos).
                ["hospital"] = new BuildingUpgradeDefinition(
                    "hospital",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("farm", 1, "Fazenda")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("farm", 2, "Fazenda")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("warehouse", 2, "Armazém")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("warehouse", 2, "Armazém"))
                    }),

                // Mercado → Castelo ≥ N; Armazém configurado.
                ["market"] = new BuildingUpgradeDefinition(
                    "market",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("warehouse", 1, "Armazém")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("warehouse", 1, "Armazém")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("warehouse", 2, "Armazém")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("warehouse", 2, "Armazém"))
                    }),

                // Templo → Castelo ≥ N; Hospital configurado.
                ["temple"] = new BuildingUpgradeDefinition(
                    "temple",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("hospital", 1, "Hospital")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("hospital", 1, "Hospital")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("hospital", 2, "Hospital")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("hospital", 2, "Hospital"))
                    }),

                // Arena → Castelo ≥ N; Academia configurada.
                ["arena"] = new BuildingUpgradeDefinition(
                    "arena",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("academy", 1, "Academia")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("academy", 2, "Academia")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("academy", 2, "Academia"))
                    }),

                // Laboratório → Castelo ≥ N; Academia + Mina.
                ["laboratory"] = new BuildingUpgradeDefinition(
                    "laboratory",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(
                            2,
                            new BuildingLevelRequirement("academy", 1, "Academia"),
                            new BuildingLevelRequirement("mine", 1, "Mina")),
                        [3] = WithBuildings(
                            3,
                            new BuildingLevelRequirement("academy", 1, "Academia"),
                            new BuildingLevelRequirement("mine", 1, "Mina")),
                        [4] = WithBuildings(
                            4,
                            new BuildingLevelRequirement("academy", 2, "Academia"),
                            new BuildingLevelRequirement("mine", 2, "Mina")),
                        [5] = WithBuildings(
                            5,
                            new BuildingLevelRequirement("academy", 2, "Academia"),
                            new BuildingLevelRequirement("mine", 2, "Mina"))
                    }),

                // Torre dos Dragões → Castelo ≥ N; Academia; Essência via custos do BuildingCatalog.
                ["dragon-tower"] = new BuildingUpgradeDefinition(
                    "dragon-tower",
                    DynamicCastle(),
                    new Dictionary<int, BuildingUpgradeRequirement>
                    {
                        [2] = WithBuildings(2, new BuildingLevelRequirement("academy", 1, "Academia")),
                        [3] = WithBuildings(3, new BuildingLevelRequirement("academy", 1, "Academia")),
                        [4] = WithBuildings(4, new BuildingLevelRequirement("academy", 2, "Academia")),
                        [5] = WithBuildings(5, new BuildingLevelRequirement("academy", 2, "Academia"))
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
