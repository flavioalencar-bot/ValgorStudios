using System;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Marches
{
    /// <summary>
    /// Cálculo de deslocamento por distância/velocidade (timestamp), sem dependência de FPS.
    /// Reutiliza a matemática de <see cref="TravelTimeCalculator"/>.
    /// </summary>
    public sealed class MarchTravelCalculator
    {
        private readonly WorldMapSettings _settings;

        public MarchTravelCalculator(WorldMapSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public TimeSpan Calculate(WorldMapNodeDefinition from, WorldMapNodeDefinition to, float? speedOverride = null)
        {
            var speed = speedOverride ?? _settings.MarchSpeedUnitsPerHour;
            if (speed <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speedOverride), "Speed deve ser positivo.");
            }

            var distance = TravelTimeCalculator.Distance(from.X, from.Z, to.X, to.Z);
            return TimeSpan.FromHours(distance / speed);
        }

        public DateTime EstimateArrival(DateTime departureAt, WorldMapNodeDefinition from, WorldMapNodeDefinition to, float? speedOverride = null) =>
            departureAt.Add(Calculate(from, to, speedOverride));
    }
}
