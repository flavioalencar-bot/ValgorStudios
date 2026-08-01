using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    public static class BuildingCatalog
    {
        private static readonly IReadOnlyDictionary<string, BuildingDefinition> Definitions =
            new Dictionary<string, BuildingDefinition>
            {
                ["castle"] = Create("castle", "Castelo", 30, gold: 200, food: 50, wood: 100, stone: 100, iron: 40, essence: 5),
                ["wall"] = Create("wall", "Muralha", 15, gold: 160, food: 20, wood: 80, stone: 140, iron: 40, essence: 0),
                ["farm"] = Create("farm", "Fazenda", 15, gold: 80, food: 20, wood: 30, stone: 0, iron: 0, essence: 0),
                ["lumbermill"] = Create("lumbermill", "Serraria", 15, gold: 100, food: 0, wood: 40, stone: 10, iron: 0, essence: 0),
                ["quarry"] = Create("quarry", "Pedreira", 15, gold: 120, food: 0, wood: 50, stone: 10, iron: 0, essence: 0),
                ["mine"] = Create("mine", "Mina", 15, gold: 150, food: 0, wood: 70, stone: 20, iron: 30, essence: 0),
                ["warehouse"] = Create("warehouse", "Armazém", 15, gold: 90, food: 10, wood: 40, stone: 20, iron: 10, essence: 0),
                ["residence"] = Create("residence", "Residência", 15, gold: 110, food: 30, wood: 60, stone: 20, iron: 0, essence: 0),
                ["market"] = Create("market", "Mercado", 12, gold: 160, food: 20, wood: 80, stone: 20, iron: 10, essence: 0),
                ["training-ground"] = Create("training-ground", "Campo de Treinamento", 12, gold: 200, food: 30, wood: 100, stone: 40, iron: 20, essence: 0),
                ["foundry"] = Create("foundry", "Fundição", 15, gold: 180, food: 0, wood: 60, stone: 40, iron: 50, essence: 0),
                ["arena"] = Create("arena", "Arena", 12, gold: 280, food: 40, wood: 130, stone: 80, iron: 40, essence: 0),
                ["academy"] = Create("academy", "Academia", 12, gold: 250, food: 40, wood: 100, stone: 40, iron: 20, essence: 0),
                ["hospital"] = Create("hospital", "Hospital", 12, gold: 180, food: 60, wood: 80, stone: 30, iron: 10, essence: 0),
                ["temple"] = Create("temple", "Templo", 12, gold: 220, food: 40, wood: 100, stone: 50, iron: 20, essence: 10),
                ["hero-hall"] = Create("hero-hall", "Salão dos Heróis", 12, gold: 300, food: 40, wood: 120, stone: 60, iron: 30, essence: 5),
                ["dragon-tower"] = Create("dragon-tower", "Torre dos Dragões", 15, gold: 400, food: 80, wood: 180, stone: 120, iron: 60, essence: 25)
            };

        public static BuildingDefinition Get(string id)
        {
            var canonical = BuildingIdMigration.ToCanonical(id);
            return Definitions[canonical];
        }

        public static IReadOnlyDictionary<string, BuildingDefinition> All => Definitions;

        public static bool TryGet(string id, out BuildingDefinition definition)
        {
            var canonical = BuildingIdMigration.ToCanonical(id);
            return Definitions.TryGetValue(canonical, out definition!);
        }

        private static BuildingDefinition Create(
            string id,
            string name,
            int maxLevel,
            long gold,
            long food,
            long wood,
            long stone,
            long iron,
            long essence) =>
            new(id, name, maxLevel, new Dictionary<ResourceType, long>
            {
                [ResourceType.Gold] = gold,
                [ResourceType.Food] = food,
                [ResourceType.Wood] = wood,
                [ResourceType.Stone] = stone,
                [ResourceType.Iron] = iron,
                [ResourceType.DragonEssence] = essence
            });
    }
}
