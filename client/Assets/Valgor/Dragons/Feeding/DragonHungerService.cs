using System;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Feeding
{
    /// <summary>
    /// Decaimento de fome e transição para HUNGRY.
    /// </summary>
    public sealed class DragonHungerService
    {
        private readonly DragonSettings _settings;
        private readonly DragonStateMachine _stateMachine;

        public DragonHungerService(DragonSettings settings, DragonStateMachine stateMachine)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        public bool CanDecay(DragonInstance dragon) =>
            dragon.State is DragonState.Ready or DragonState.Resting or DragonState.Juvenile;

        public bool ApplyDecay(DragonInstance dragon, DragonDefinition definition, DateTime nowUtc)
        {
            if (!CanDecay(dragon))
            {
                return false;
            }

            var elapsed = nowUtc - dragon.LastUpdatedUtc;
            if (elapsed.TotalHours < _settings.HungerIntervalHours)
            {
                return false;
            }

            var ticks = (int)Math.Floor(elapsed.TotalHours / _settings.HungerIntervalHours);
            if (ticks <= 0)
            {
                return false;
            }

            var previous = dragon.State;
            dragon.Hunger = Math.Max(0, dragon.Hunger - ticks * _settings.HungerDecayPerTick);
            dragon.LastUpdatedUtc = dragon.LastUpdatedUtc.AddHours(ticks * _settings.HungerIntervalHours);

            var threshold = (int)Math.Floor(definition.MaxHunger * _settings.HungryThresholdRatio);
            if (dragon.Hunger <= threshold &&
                dragon.State is DragonState.Ready or DragonState.Resting or DragonState.Juvenile)
            {
                _stateMachine.TryTransition(dragon, DragonState.Hungry, out _);
            }

            return previous != dragon.State || ticks > 0;
        }

        public bool IsReadyHunger(DragonInstance dragon, DragonDefinition definition) =>
            dragon.Hunger >= (int)Math.Ceiling(definition.MaxHunger * _settings.ReadyHungerRatio);
    }
}
