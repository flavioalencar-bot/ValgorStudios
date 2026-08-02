using System;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Parâmetros ambientais da Capital (sem layout lógico de edifícios).
    /// </summary>
    public static class CityCapitalEnvironmentConfig
    {
        public const float ImperialRoadWidth = 3.6f;
        public const float PlazaCenterZ = -3.2f;
        public const float CastlePlatformY = 0.08f;
        public const float DragonPlateauY = 1.2f;

        public static int VegetationCount(CityAmbientQuality quality, int environmentTier)
        {
            var baseCount = quality switch
            {
                CityAmbientQuality.Low => 8,
                CityAmbientQuality.Medium => 14,
                CityAmbientQuality.High => 22,
                _ => 14
            };
            return Math.Clamp(baseCount + Math.Max(0, environmentTier - 1) * 2, 6, 36);
        }

        public static int DistrictPropBudget(CityAmbientQuality quality, int environmentTier)
        {
            var budget = quality switch
            {
                CityAmbientQuality.Low => 10,
                CityAmbientQuality.Medium => 22,
                CityAmbientQuality.High => 36,
                _ => 22
            };
            return budget + Math.Max(0, environmentTier - 1) * 2;
        }

        public static bool AllowHighExclusiveProps =>
            CityAmbientQualitySettings.Current >= CityAmbientQuality.High;

        public static bool AllowExtraParticles =>
            CityAmbientQualitySettings.Current >= CityAmbientQuality.High;

        public static float FlagDensityMultiplier(int environmentTier) =>
            environmentTier switch
            {
                <= 1 => 0.5f,
                <= 3 => 1f,
                <= 5 => 1.35f,
                _ => 1.7f
            };

        public static float SmokeScale(CityAmbientQuality quality) =>
            quality switch
            {
                CityAmbientQuality.Low => 0f,
                CityAmbientQuality.Medium => 1f,
                CityAmbientQuality.High => 1.45f,
                _ => 1f
            };

        /// <summary>Densidade visual por tier (1–6) — só ambientação.</summary>
        public static float TierVisualDensity(int tier) =>
            Math.Clamp(0.35f + tier * 0.12f, 0.35f, 1.1f);
    }
}
