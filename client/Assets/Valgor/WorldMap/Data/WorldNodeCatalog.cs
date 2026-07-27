using System.Collections.Generic;
using System.Linq;
using System;
using Valgor.City.Data;

namespace Valgor.WorldMap.Data
{
    public static class WorldNodeCatalog
    {
        private static readonly Dictionary<string, WorldMapNodeDefinition> Nodes = Build();

        public static IReadOnlyDictionary<string, WorldMapNodeDefinition> All => Nodes;

        public static WorldMapNodeDefinition Get(string id) => Nodes[id];

        public static IEnumerable<WorldMapNodeDefinition> ForRegion(string regionId) =>
            Nodes.Values.Where(node => node.RegionId == regionId);

        private static Dictionary<string, WorldMapNodeDefinition> Build()
        {
            var settings = WorldMapSettings.Default;
            var nodes = new List<WorldMapNodeDefinition>
            {
                new WorldCityNode(
                    settings.PlayerHomeNodeId,
                    "home",
                    "Cidade do Jogador",
                    "Sua base. Origem das marchas e retorno do exército.",
                    WorldNodeStatus.Available,
                    settings.PlayerHomeX,
                    settings.PlayerHomeZ,
                    isPlayerHome: true),

                new WorldVillageNode("forest-village", "forest", "Aldeia Verde", "Vilarejo madeireiro amistoso.", WorldNodeStatus.Available, -14f, 6f, 420),
                new WorldResourceNode("forest-wood", "forest", "Bosque de Carvalho", "Madeira acumulada na clareira.", WorldNodeStatus.Available, -10f, 10f, ResourceType.Wood, 800, 1, 200, TimeSpan.FromHours(4)),
                new WorldCreatureNode("forest-wolf", "forest", "Matilha Sombria", "Lobos territoriais.", WorldNodeStatus.Available, -8f, 5f, 2, "wolf-pack"),
                new WorldLandmarkNode("forest-stone", "forest", "Menir Antigo", "Pedra ritual da primeira era.", WorldNodeStatus.Available, -16f, 11f, "menhir"),

                new WorldResourceNode("mount-iron", "mountains", "Veio de Ferro", "Minério a céu aberto.", WorldNodeStatus.Available, 12f, 10f, ResourceType.Iron, 500, 1, 150, TimeSpan.FromHours(5)),
                new WorldCreatureNode("mount-golem", "mountains", "Golem de Xisto", "Guardião das encostas.", WorldNodeStatus.Available, 8f, 14f, 4, "shale-golem"),
                new WorldDragonNode("mount-dragon", "mountains", "Ninho Cinzento", "Presença dracônica adormecida.", WorldNodeStatus.Available, 14f, 15f, "ash-drake"),
                new WorldLandmarkNode("mount-peak", "mountains", "Pico do Eco", "Mirante natural.", WorldNodeStatus.Available, 6f, 9f, "echo-peak"),

                new WorldVillageNode("coast-harbor", "coast", "Porto de Âmbar", "Comércio costeiro.", WorldNodeStatus.Available, -16f, -4f, 780),
                new WorldResourceNode("coast-gold", "coast", "Banco de Conchas", "Ouro lavado pela maré.", WorldNodeStatus.Available, -11f, -8f, ResourceType.Gold, 350, 1, 120, TimeSpan.FromHours(3)),
                new WorldCreatureNode("coast-crab", "coast", "Caranguejo Titã", "Ameaça das praias.", WorldNodeStatus.Available, -18f, -9f, 3, "tide-crab"),
                new WorldCityNode("coast-outpost", "coast", "Posto Mercante", "Cidade aliada do litoral.", WorldNodeStatus.Available, -12f, -3f),

                new WorldResourceNode("desert-stone", "desert", "Pedreira de Vidro", "Bloqueada pelo calor.", WorldNodeStatus.Locked, 14f, -8f, ResourceType.Stone, 600, 2, 180, TimeSpan.FromHours(6)),
                new WorldCreatureNode("desert-scorpion", "desert", "Escorpião de Areia", "Região trancada.", WorldNodeStatus.Locked, 10f, -12f, 5, "glass-scorpion"),
                new WorldLandmarkNode("ruins-obelisk", "ruins", "Obelisco do Éter", "Acesso futuro.", WorldNodeStatus.Locked, 1f, 2f, "ether-obelisk"),
                new WorldDragonNode("portal-guardian", "portal", "Guardião do Portal", "Requer progresso avançado.", WorldNodeStatus.Locked, 3f, 17f, "portal-wyrm")
            };

            return nodes.ToDictionary(node => node.Id);
        }
    }
}
