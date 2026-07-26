using System;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Growth
{
    /// <summary>
    /// Vínculo jogador↔dragão (nível e pontos).
    /// </summary>
    public sealed class DragonBondService
    {
        private readonly DragonSettings _settings;

        public DragonBondService(DragonSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static double PowerMultiplier(int bondLevel) =>
            1.0 + Math.Max(0, bondLevel) * 0.05;

        public void AddBondPoints(DragonInstance dragon, int points)
        {
            if (points <= 0 || dragon.BondLevel >= _settings.MaxBondLevel)
            {
                return;
            }

            dragon.BondPoints += points;
            while (dragon.BondLevel < _settings.MaxBondLevel &&
                   dragon.BondPoints >= _settings.BondPointsPerLevel)
            {
                dragon.BondPoints -= _settings.BondPointsPerLevel;
                dragon.BondLevel++;
            }

            if (dragon.BondLevel >= _settings.MaxBondLevel)
            {
                dragon.BondPoints = 0;
            }
        }
    }
}
