using System;
using System.Collections.Generic;
using Valgor.City.Data;

namespace Valgor.WorldMap.Creatures
{
    public static class WorldCreatureCatalog
    {
        private static readonly Dictionary<string, CreatureRewardTable> RewardTables = new()
        {
            ["beast-basic"] = new("beast-basic", new[]
            {
                new CreatureRewardEntry(ResourceType.Food, 120),
                new CreatureRewardEntry(ResourceType.Gold, 40)
            }),
            ["construct-ore"] = new("construct-ore", new[]
            {
                new CreatureRewardEntry(ResourceType.Iron, 90),
                new CreatureRewardEntry(ResourceType.Stone, 60)
            }),
            ["coastal"] = new("coastal", new[]
            {
                new CreatureRewardEntry(ResourceType.Gold, 80),
                new CreatureRewardEntry(ResourceType.Food, 50)
            }),
            ["desert-elite"] = new("desert-elite", new[]
            {
                new CreatureRewardEntry(ResourceType.Stone, 140),
                new CreatureRewardEntry(ResourceType.DragonEssence, 5)
            })
        };

        private static readonly Dictionary<string, WorldCreatureDefinition> Creatures = Build();

        public static IReadOnlyDictionary<string, WorldCreatureDefinition> All => Creatures;

        public static WorldCreatureDefinition Get(string id) => Creatures[id];

        public static bool TryGet(string id, out WorldCreatureDefinition definition) =>
            Creatures.TryGetValue(id, out definition!);

        public static CreatureRewardTable GetRewardTable(string id) => RewardTables[id];

        private static Dictionary<string, WorldCreatureDefinition> Build()
        {
            return new Dictionary<string, WorldCreatureDefinition>
            {
                ["forest-wolf"] = new(
                    "forest-wolf",
                    WorldCreatureType.Beast,
                    "Matilha Sombria",
                    level: 2,
                    recommendedPower: 40,
                    energyCost: 8,
                    respawnDuration: TimeSpan.FromHours(2),
                    regionId: "forest",
                    x: -8f,
                    z: 5f,
                    rewards: RewardTables["beast-basic"]),

                ["mount-golem"] = new(
                    "mount-golem",
                    WorldCreatureType.Construct,
                    "Golem de Xisto",
                    level: 4,
                    recommendedPower: 90,
                    energyCost: 14,
                    respawnDuration: TimeSpan.FromHours(4),
                    regionId: "mountains",
                    x: 8f,
                    z: 14f,
                    rewards: RewardTables["construct-ore"]),

                ["coast-crab"] = new(
                    "coast-crab",
                    WorldCreatureType.Beast,
                    "Caranguejo Titã",
                    level: 3,
                    recommendedPower: 65,
                    energyCost: 10,
                    respawnDuration: TimeSpan.FromHours(3),
                    regionId: "coast",
                    x: -18f,
                    z: -9f,
                    rewards: RewardTables["coastal"]),

                ["desert-scorpion"] = new(
                    "desert-scorpion",
                    WorldCreatureType.Aberration,
                    "Escorpião de Areia",
                    level: 5,
                    recommendedPower: 130,
                    energyCost: 18,
                    respawnDuration: TimeSpan.FromHours(6),
                    regionId: "desert",
                    x: 10f,
                    z: -12f,
                    rewards: RewardTables["desert-elite"],
                    startsLocked: true)
            };
        }
    }
}
