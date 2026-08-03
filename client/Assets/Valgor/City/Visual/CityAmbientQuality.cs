namespace Valgor.City.Visual
{
    /// <summary>
    /// Qualidade de efeitos ambientais (bandeiras, fumaça, água, tochas).
    /// </summary>
    public enum CityAmbientQuality
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public static class CityAmbientQualitySettings
    {
        public static CityAmbientQuality Current { get; private set; } = CityAmbientQuality.Medium;

        public static void Set(CityAmbientQuality quality) => Current = quality;

        public static bool AllowFlags => Current >= CityAmbientQuality.Low;
        public static bool AllowTorches => Current >= CityAmbientQuality.Medium;
        public static bool AllowSmoke => Current >= CityAmbientQuality.Medium;
        public static bool AllowWaterFx => Current >= CityAmbientQuality.Medium;
        public static bool AllowBirds => Current >= CityAmbientQuality.High;
        public static bool AllowButterflies => Current >= CityAmbientQuality.Medium;
        public static bool AllowFishHints => Current >= CityAmbientQuality.High;
        public static bool AllowFireflies => Current >= CityAmbientQuality.High;
        public static bool AllowNpcs => Current >= CityAmbientQuality.Medium;
        public static bool AllowFlagMotion => Current >= CityAmbientQuality.Medium;
        public static bool AllowLightFog => Current >= CityAmbientQuality.Medium;
        public static bool AllowDustLeaves => Current >= CityAmbientQuality.High;
        public static bool AllowDistrictAudio => Current >= CityAmbientQuality.Medium;
        public static bool AllowDragonIdleMotion => Current >= CityAmbientQuality.Medium;
        public static bool AllowDragonExtraMotion => Current >= CityAmbientQuality.High;

        public static bool IsValid(CityAmbientQuality quality) =>
            quality is CityAmbientQuality.Low or CityAmbientQuality.Medium or CityAmbientQuality.High;
    }
}
