namespace Valgor.City.Visual
{
    /// <summary>Sprint 2 — Distrito Real (testável sem UnityEngine).</summary>
    public static class CityRoyalDistrictConfig
    {
        public const bool Enabled = true;
        public const string RootName = "RoyalDistrict";

        public const float PlazaRadius = 6.8f;
        public const float PlazaRingCount = 4;
        public const int RadialPathCount = 8;
        public const int FountainSides = 8;
        public const int GrandStairSteps = 8;
        public const int NobleResidenceCount = 8;
        public const int FormalGardenBeds = 8;
        public const int OrnamentalTreePairs = 6;
        public const int LampPostCount = 10;
        public const int PropBudget = 24;

        /// <summary>Exclusão de casas vernaculars ao redor do Castelo.</summary>
        public const float VernacularExclusionRadius = 9.5f;

        public static readonly string[] Features =
        {
            "praca-imperial", "escadaria-real", "fonte-monumental", "jardins-imperiais",
            "monumento-valgor", "residencias-nobres", "iluminacao", "ruas-pedra",
            "vegetacao-formal", "props"
        };
    }
}
