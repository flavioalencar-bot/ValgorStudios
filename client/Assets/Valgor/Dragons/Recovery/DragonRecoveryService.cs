using System;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Recovery
{
    public sealed class DragonRecoveryService
    {
        private readonly DragonSettings _settings;
        private readonly DragonStateMachine _stateMachine;

        public DragonRecoveryService(DragonSettings settings, DragonStateMachine stateMachine)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        public bool TryStartRecovery(DragonInstance dragon, DateTime nowUtc, out string error)
        {
            if (dragon.State is not (DragonState.Exhausted or DragonState.Injured))
            {
                error = "Dragão não precisa de recuperação.";
                return false;
            }

            if (!_stateMachine.TryTransition(dragon, DragonState.Recovering, out error))
            {
                return false;
            }

            dragon.StateEndsAtUtc = nowUtc.AddHours(_settings.RecoveryDurationHours);
            return true;
        }

        public void Advance(DragonInstance dragon, DateTime nowUtc)
        {
            if (dragon.State == DragonState.Recovering &&
                dragon.StateEndsAtUtc.HasValue &&
                nowUtc >= dragon.StateEndsAtUtc.Value)
            {
                if (_stateMachine.TryTransition(dragon, DragonState.Resting, out _))
                {
                    dragon.StateEndsAtUtc = null;
                }
            }

            if (dragon.State == DragonState.Hatching &&
                dragon.StateEndsAtUtc.HasValue &&
                nowUtc >= dragon.StateEndsAtUtc.Value)
            {
                if (_stateMachine.TryTransition(dragon, DragonState.Resting, out _))
                {
                    dragon.StateEndsAtUtc = null;
                }
            }
        }
    }
}
