using System;

namespace Valgor.WorldMap.Creatures
{
    public static class CreatureDifficultyResolver
    {
        public static CreatureDifficultyBand Resolve(int attackerPower, int recommendedPower)
        {
            if (recommendedPower <= 0)
            {
                return CreatureDifficultyBand.Trivial;
            }

            var ratio = attackerPower / (double)recommendedPower;
            if (ratio >= 1.5)
            {
                return CreatureDifficultyBand.Trivial;
            }

            if (ratio >= 1.15)
            {
                return CreatureDifficultyBand.Easy;
            }

            if (ratio >= 0.85)
            {
                return CreatureDifficultyBand.Fair;
            }

            if (ratio >= 0.6)
            {
                return CreatureDifficultyBand.Hard;
            }

            return CreatureDifficultyBand.Impossible;
        }

        /// <summary>
        /// Resolução provisória sem combate visual: Impossible falha; demais têm sucesso determinístico.
        /// </summary>
        public static bool CanDefeatProvisional(int attackerPower, int recommendedPower) =>
            Resolve(attackerPower, recommendedPower) != CreatureDifficultyBand.Impossible;
    }
}
