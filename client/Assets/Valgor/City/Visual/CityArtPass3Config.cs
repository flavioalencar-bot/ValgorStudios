namespace Valgor.City.Visual
{
    /// <summary>Constantes do Art Pass 3.0 — Capital completa (testável sem UnityEngine).</summary>
    public static class CityArtPass3Config
    {
        public const bool Enabled = true;
        public const bool ClosedWallEnabled = true;
        public const bool VernacularHousingEnabled = true;
        public const bool SoftDistrictPadsEnabled = true;
        public const bool LayeredVegetationEnabled = true;
        public const bool DistrictDressingEnabled = true;
        public const bool RoadCobbleEnabled = true;
        public const bool WaterPolishEnabled = true;
        public const bool AtmosphereMarkersEnabled = true;

        public const int WallSegments = 28;
        public const int CornerTowerCount = 4;
        public const int VernacularHouseRings = 5;
        public const int NatureCanopyLayers = 3;
        public const int StaticNpcMarkerCount = 12;
        public const int DistrictDressingPropBudget = 48;

        /// <summary>HF já presentes via catálogo / loaders / proxies Sprint 3.</summary>
        public static readonly string[] HighFidelityAssets =
        {
            "wall-gate", "dragon-tower", "castle-tiers",
            "market", "warehouse", "hospital", "arena", "academy", "training-ground",
            "residence", "foundry", "temple", "hero-hall", "farm", "lumbermill", "quarry", "mine"
        };

        /// <summary>Ainda procedurais ambientais (não são BuildingId oficiais).</summary>
        public static readonly string[] RemainingBlockout =
        {
            "vegetacao-primitivas", "estradas-flat",
            "agua-flat", "props-distrito", "npcs-capsula", "horizonte-esferas"
        };

        public static readonly string[] Stages =
        {
            "muralha-fechada", "blockout-vernacular", "distrito-real", "distrito-dragoes",
            "distrito-economico", "distrito-militar", "vegetacao", "estradas", "agua", "atmosfera"
        };
    }
}
