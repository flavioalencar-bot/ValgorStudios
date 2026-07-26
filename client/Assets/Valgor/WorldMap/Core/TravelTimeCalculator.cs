using System;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    public static class TravelTimeCalculator
    {
        public static double Distance(float x1, float z1, float x2, float z2)
        {
            var dx = x2 - x1;
            var dz = z2 - z1;
            return Math.Sqrt((dx * dx) + (dz * dz));
        }

        public static TimeSpan Calculate(float fromX, float fromZ, float toX, float toZ, WorldMapSettings settings)
        {
            if (settings.MarchSpeedUnitsPerHour <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(settings), "MarchSpeedUnitsPerHour deve ser positivo.");
            }

            var distance = Distance(fromX, fromZ, toX, toZ);
            var hours = distance / settings.MarchSpeedUnitsPerHour;
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Calculate(WorldMapNodeDefinition from, WorldMapNodeDefinition to, WorldMapSettings settings) =>
            Calculate(from.X, from.Z, to.X, to.Z, settings);
    }
}
