namespace Valgor.WorldMap.Data
{
    /// <summary>
    /// Parâmetros configuráveis do mapa mundial (sem constantes mágicas espalhadas).
    /// </summary>
    public sealed class WorldMapSettings
    {
        public static WorldMapSettings Default { get; } = new();

        public float MarchSpeedUnitsPerHour { get; init; } = 8f;
        public float PlayerHomeX { get; init; }
        public float PlayerHomeZ { get; init; } = -14f;
        public string PlayerHomeNodeId { get; init; } = "home-city";
        public string PersistenceKey { get; init; } = "valgor.worldmap.v1";
        public double MarchTickIntervalSeconds { get; init; } = 1.0;
    }
}
