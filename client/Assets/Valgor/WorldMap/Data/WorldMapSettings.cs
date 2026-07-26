namespace Valgor.WorldMap.Data
{
    /// <summary>
    /// Parâmetros configuráveis do mapa mundial (sem constantes mágicas espalhadas).
    /// </summary>
    public sealed class WorldMapSettings
    {
        public static WorldMapSettings Default { get; } = new();

        public float MarchSpeedUnitsPerHour { get; set; } = 8f;
        public float PlayerHomeX { get; set; }
        public float PlayerHomeZ { get; set; } = -14f;
        public string PlayerHomeNodeId { get; set; } = "home-city";
        public string PersistenceKey { get; set; } = "valgor.worldmap.v1";
        public double MarchTickIntervalSeconds { get; set; } = 1.0;
        public int StartingEnergy { get; set; } = 100;
        public int MaxEnergy { get; set; } = 100;
        public double EnergyRegenIntervalSec { get; set; } = 60;
        public int EnergyRegenAmount { get; set; } = 1;
        public int MarchDispatchEnergyCost { get; set; }
        public string DefaultPlayerId { get; set; } = "local-player";
        public long DefaultMarchCapacity { get; set; } = 10_000;
        public string EnergyPersistenceKey { get; set; } = "valgor.worldmap.energy.v1";
        public string FilterPersistenceKey { get; set; } = "valgor.worldmap.filters.v1";
        public string CameraPersistenceKey { get; set; } = "valgor.worldmap.camera.v1";
        public float LocateDefaultZoom { get; set; } = 14f;
        public float LocateHomeZoom { get; set; } = 12f;
        public float DefaultCameraZoom { get; set; } = 14f;
        public bool TerritoryOverlayEnabled { get; set; } = true;
    }
}
