using System;
using System.Collections.Generic;

namespace Valgor.WorldMap.Data
{
    public enum RegionStatus
    {
        Locked,
        Available,
        Cleared
    }

    public sealed class RegionDefinition
    {
        public RegionDefinition(string id, string displayName, string description, RegionStatus status, float x, float z)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? string.Empty;
            DefaultStatus = status;
            X = x;
            Z = z;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public RegionStatus DefaultStatus { get; }
        public float X { get; }
        public float Z { get; }
    }

    public sealed class RegionInstance
    {
        public RegionInstance(string definitionId, RegionStatus status)
        {
            DefinitionId = definitionId;
            Status = status;
        }

        public string DefinitionId { get; }
        public RegionStatus Status { get; set; }
    }

    public static class WorldMapCatalog
    {
        private static readonly Dictionary<string, RegionDefinition> Regions = new()
        {
            ["forest"] = new("forest", "Floresta de Valgor", "Bosques densos e rotas iniciais.", RegionStatus.Available, -12f, 8f),
            ["mountains"] = new("mountains", "Montanhas Cinzentas", "Minérios e riscos elevados.", RegionStatus.Available, 10f, 12f),
            ["coast"] = new("coast", "Costa de Âmbar", "Comércio e vento constante.", RegionStatus.Available, -14f, -6f),
            ["desert"] = new("desert", "Deserto de Vidro", "Calor extremo e ruínas.", RegionStatus.Locked, 12f, -10f),
            ["ruins"] = new("ruins", "Ruínas do Éter", "Relíquias e anomalias.", RegionStatus.Locked, 0f, 0f),
            ["portal"] = new("portal", "Portal Ancestral", "Acesso avançado ao interior.", RegionStatus.Locked, 2f, 16f)
        };

        public static IReadOnlyDictionary<string, RegionDefinition> All => Regions;
        public static RegionDefinition Get(string id) => Regions[id];
    }
}
