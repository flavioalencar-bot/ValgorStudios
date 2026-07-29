using System;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Recovery
{
    /// <summary>
    /// Hatch, maturação juvenil, recuperação e descanso com timers determinísticos.
    /// </summary>
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

        public bool TryBeginRest(DragonInstance dragon, DateTime nowUtc, out string error)
        {
            if (dragon.State is not (DragonState.Ready or DragonState.Hungry))
            {
                error = "Descanso indisponível neste estado.";
                return false;
            }

            if (!_stateMachine.TryTransition(dragon, DragonState.Resting, out error))
            {
                return false;
            }

            dragon.StateEndsAtUtc = nowUtc.AddHours(_settings.RestDurationHours);
            return true;
        }

        public void BeginTimedState(DragonInstance dragon, DateTime nowUtc, double durationHours) =>
            dragon.StateEndsAtUtc = nowUtc.AddHours(durationHours);

        public void Advance(
            DragonInstance dragon,
            DateTime nowUtc,
            Func<DragonInstance, bool>? isReadyHunger = null,
            Func<DragonInstance, bool>? canCompleteHatch = null)
        {
            if (!dragon.StateEndsAtUtc.HasValue || nowUtc < dragon.StateEndsAtUtc.Value)
            {
                // Descanso sem timer explícito ainda pode completar por fome suficiente.
                if (dragon.State == DragonState.Resting &&
                    isReadyHunger != null &&
                    isReadyHunger(dragon) &&
                    !dragon.StateEndsAtUtc.HasValue)
                {
                    _stateMachine.TryTransition(dragon, DragonState.Ready, out _);
                }

                return;
            }

            switch (dragon.State)
            {
                case DragonState.Hatching:
                    // Fase 1: incubação só conclui com cuidados suficientes.
                    if (canCompleteHatch != null && !canCompleteHatch(dragon))
                    {
                        break;
                    }

                    if (_stateMachine.TryTransition(dragon, DragonState.Juvenile, out _))
                    {
                        if (dragon.DragonLevel < 1)
                        {
                            dragon.DragonLevel = 1;
                        }

                        dragon.StateEndsAtUtc = nowUtc.AddHours(_settings.JuvenileDurationHours);
                    }

                    break;

                case DragonState.Juvenile:
                    if (_stateMachine.TryTransition(dragon, DragonState.Resting, out _))
                    {
                        dragon.StateEndsAtUtc = nowUtc.AddHours(_settings.RestDurationHours);
                    }

                    break;

                case DragonState.Recovering:
                    if (_stateMachine.TryTransition(dragon, DragonState.Resting, out _))
                    {
                        dragon.StateEndsAtUtc = nowUtc.AddHours(_settings.RestDurationHours);
                    }

                    break;

                case DragonState.Resting:
                    if (isReadyHunger == null || isReadyHunger(dragon))
                    {
                        if (_stateMachine.TryTransition(dragon, DragonState.Ready, out _))
                        {
                            dragon.StateEndsAtUtc = null;
                        }
                    }
                    else
                    {
                        // Sem fome suficiente: permanece descansando até alimentar.
                        dragon.StateEndsAtUtc = null;
                    }

                    break;
            }
        }
    }
}
