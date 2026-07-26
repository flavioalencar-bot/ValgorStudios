using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    public static class BuildingCatalog
    {
        private static readonly IReadOnlyDictionary<string, BuildingDefinition> Definitions =
            new Dictionary<string, BuildingDefinition>
            {
                ["castle"] = Create("castle", "Castelo", 20, 200, 100, 100),
                ["farm"] = Create("farm", "Fazenda", 15, 80, 30, 0),
                ["lumbermill"] = Create("lumbermill", "Serraria", 15, 100, 40, 10),
                ["quarry"] = Create("quarry", "Pedreira", 15, 120, 50, 10),
                ["mine"] = Create("mine", "Mina", 15, 150, 70, 20),
                ["warehouse"] = Create("warehouse", "Armazém", 15, 90, 40, 0),
                ["academy"] = Create("academy", "Academia", 12, 250, 100, 40),
                ["institute"] = Create("institute", "Instituto", 12, 300, 120, 60),
                ["hospital"] = Create("hospital", "Hospital", 12, 180, 80, 30),
                ["market"] = Create("market", "Mercado", 12, 160, 80, 20),
                ["temple"] = Create("temple", "Templo", 12, 220, 100, 50),
                ["dragon-tower"] = Create("dragon-tower", "Torre dos Dragões", 10, 400, 180, 120),
                ["arena"] = Create("arena", "Arena", 12, 280, 130, 80),
                ["laboratory"] = Create("laboratory", "Laboratório", 12, 350, 160, 100)
            };

        public static BuildingDefinition Get(string id) => Definitions[id];

        private static BuildingDefinition Create(string id, string name, int maxLevel, long gold, long wood, long stone) =>
            new(id, name, maxLevel, new Dictionary<ResourceType, long>
            {
                [ResourceType.Gold] = gold,
                [ResourceType.Wood] = wood,
                [ResourceType.Stone] = stone
            });
    }
}
