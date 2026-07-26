using System;
using System.Collections.Generic;

namespace Valgor.WorldMap.Territory
{
    public enum WorldTerritoryState
    {
        Neutral,
        Owned,
        Allied,
        Enemy,
        Contested,
        Locked
    }

    public readonly struct TerritoryColor
    {
        public TerritoryColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
    }

    public sealed class WorldTerritoryDefinition
    {
        public WorldTerritoryDefinition(
            string id,
            string regionId,
            string displayName,
            WorldTerritoryState defaultState,
            float centerX,
            float centerZ)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            RegionId = regionId ?? throw new ArgumentNullException(nameof(regionId));
            DisplayName = displayName ?? string.Empty;
            DefaultState = defaultState;
            CenterX = centerX;
            CenterZ = centerZ;
        }

        public string Id { get; }
        public string RegionId { get; }
        public string DisplayName { get; }
        public WorldTerritoryState DefaultState { get; }
        public float CenterX { get; }
        public float CenterZ { get; }
    }

    /// <summary>
    /// Estado runtime de um território (fundação; conquista vem em sprint futura).
    /// </summary>
    public sealed class WorldTerritoryRuntime
    {
        public WorldTerritoryRuntime(string definitionId, WorldTerritoryState state)
        {
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            State = state;
        }

        public string DefinitionId { get; }
        public WorldTerritoryState State { get; set; }
    }

    public static class WorldTerritoryColorResolver
    {
        public static TerritoryColor Resolve(WorldTerritoryState state) =>
            state switch
            {
                WorldTerritoryState.Owned => new TerritoryColor(0.2f, 0.55f, 0.95f, 0.4f),
                WorldTerritoryState.Allied => new TerritoryColor(0.25f, 0.75f, 0.45f, 0.4f),
                WorldTerritoryState.Enemy => new TerritoryColor(0.85f, 0.25f, 0.22f, 0.4f),
                WorldTerritoryState.Contested => new TerritoryColor(0.9f, 0.65f, 0.15f, 0.4f),
                WorldTerritoryState.Locked => new TerritoryColor(0.28f, 0.3f, 0.34f, 0.45f),
                _ => new TerritoryColor(0.55f, 0.58f, 0.6f, 0.28f)
            };
    }

    public static class WorldTerritoryCatalog
    {
        private static readonly Dictionary<string, WorldTerritoryDefinition> Territories = new()
        {
            ["territory-forest"] = new("territory-forest", "forest", "Território da Floresta", WorldTerritoryState.Owned, -12f, 8f),
            ["territory-mountains"] = new("territory-mountains", "mountains", "Território das Montanhas", WorldTerritoryState.Neutral, 10f, 12f),
            ["territory-coast"] = new("territory-coast", "coast", "Território da Costa", WorldTerritoryState.Allied, -14f, -6f),
            ["territory-desert"] = new("territory-desert", "desert", "Território do Deserto", WorldTerritoryState.Locked, 12f, -10f),
            ["territory-ruins"] = new("territory-ruins", "ruins", "Território das Ruínas", WorldTerritoryState.Enemy, 0f, 0f),
            ["territory-portal"] = new("territory-portal", "portal", "Território do Portal", WorldTerritoryState.Contested, 2f, 16f)
        };

        public static IReadOnlyDictionary<string, WorldTerritoryDefinition> All => Territories;

        public static bool TryGetByRegion(string regionId, out WorldTerritoryDefinition definition)
        {
            foreach (var pair in Territories)
            {
                if (string.Equals(pair.Value.RegionId, regionId, StringComparison.Ordinal))
                {
                    definition = pair.Value;
                    return true;
                }
            }

            definition = null!;
            return false;
        }
    }
}
