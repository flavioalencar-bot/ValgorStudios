using System;
using UnityEngine;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.SpecialPowers
{
    [Serializable]
    public sealed class HeroRuntimeState
    {
        public string HeroId;
        public SpecialPowerState SpecialState = SpecialPowerState.Ready;
        public double ActiveUntilServerTime;
        public double CooldownUntilServerTime;
    }

    public sealed class SpecialPowerStateMachine
    {
        public SpecialPowerState Evaluate(double serverNow, HeroRuntimeState runtime)
        {
            if (runtime.ActiveUntilServerTime > serverNow)
            {
                return SpecialPowerState.Active;
            }

            if (runtime.CooldownUntilServerTime > serverNow)
            {
                return SpecialPowerState.Cooldown;
            }

            return SpecialPowerState.Ready;
        }

        public bool CanActivate(SpecialPowerState state) => state == SpecialPowerState.Ready;

        public void ApplyServerTimestamps(HeroRuntimeState runtime, double activeUntil, double cooldownUntil, double serverNow)
        {
            runtime.ActiveUntilServerTime = activeUntil;
            runtime.CooldownUntilServerTime = cooldownUntil;
            runtime.SpecialState = Evaluate(serverNow, runtime);
        }

        /// <summary>
        /// Client-side prediction only. Authoritative result must come from the backend.
        /// </summary>
        public void PredictLocalActivation(HeroRuntimeState runtime, SpecialPowerDefinitionSO power, double serverNow)
        {
            runtime.ActiveUntilServerTime = serverNow + power.ActiveDurationSec;
            runtime.CooldownUntilServerTime = serverNow + power.CooldownSec;
            runtime.SpecialState = SpecialPowerState.Active;
        }
    }

    public sealed class SpecialPowerController : MonoBehaviour
    {
        [SerializeField] private SpecialPowerDefinitionSO power;
        [SerializeField] private string heroId;

        private readonly SpecialPowerStateMachine _machine = new();
        private readonly HeroRuntimeState _runtime = new();

        public HeroRuntimeState Runtime => _runtime;
        public SpecialPowerDefinitionSO Power => power;

        private void Awake()
        {
            _runtime.HeroId = string.IsNullOrWhiteSpace(heroId) && power != null ? power.HeroId : heroId;
        }

        public void SyncFromServer(double activeUntil, double cooldownUntil, double serverNow)
        {
            _machine.ApplyServerTimestamps(_runtime, activeUntil, cooldownUntil, serverNow);
        }

        public bool TryPredictActivate(double serverNow)
        {
            if (power == null) return false;
            var state = _machine.Evaluate(serverNow, _runtime);
            if (!_machine.CanActivate(state)) return false;
            _machine.PredictLocalActivation(_runtime, power, serverNow);
            return true;
        }

        public SpecialPowerState Evaluate(double serverNow) =>
            _machine.Evaluate(serverNow, _runtime);
    }
}
