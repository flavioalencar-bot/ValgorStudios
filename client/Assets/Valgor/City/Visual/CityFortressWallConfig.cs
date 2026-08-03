namespace Valgor.City.Visual
{
    /// <summary>Sprint 1 — Capital Fortress (config testável sem UnityEngine).</summary>
    public static class CityFortressWallConfig
    {
        public const bool Enabled = true;
        public const string KitId = "VAL_Capital_FortressWallKit_v1";
        public const string ResourcesMaterialKey = "Valgor/M_VAL_Capital_Architecture_WallGate";
        public const string ResourcesModulePrefix = "Valgor/WallModules/";

        public const float WallHeight = 5.8f;
        public const float WallThickness = 1.85f;
        public const float WalkWidth = 2.15f;
        public const float MerlonHeight = 0.65f;
        public const float FoundationDepth = 0.55f;
        public const float GateOpeningHalfAngleDeg = 16f;
        public const int RingSegments = 32;
        public const int LodLevels = 4;

        public static readonly string[] ModuleIds =
        {
            "WallStraight4m",
            "WallStraight8m",
            "WallCorner45",
            "WallCorner90",
            "WallTowerSmall",
            "WallTowerMedium",
            "WallTowerLarge",
            "WallGate",
            "WallWalk",
            "WallStairs",
            "WallEnd",
            "WallBastion"
        };

        public static readonly string[] TowerCycle =
        {
            "WallTowerSmall",
            "WallTowerMedium",
            "WallTowerLarge",
            "WallTowerMedium"
        };

        public const int ModuleCount = 12;
        public const int CornerCount = 4;
        public const int BastionCount = 4;
    }
}
