using NUnit.Framework;
using UnityEngine;
using Valgor.Heroes.Data;
using Valgor.Heroes.SpecialPowers;

namespace Valgor.Heroes.Tests
{
    public sealed class SpecialPowerEditModeTests
    {
        [Test]
        public void Cannot_Activate_During_Cooldown_Prediction()
        {
            var power = ScriptableObject.CreateInstance<SpecialPowerDefinitionSO>();
            power.Id = "POWER_HERO_ELYRA_001";
            power.HeroId = "HERO_ELYRA_001";
            power.ActiveDurationSec = 10f;
            power.CooldownSec = 35f;

            var machine = new SpecialPowerStateMachine();
            var runtime = new HeroRuntimeState { HeroId = power.HeroId };
            const double now = 1000d;
            machine.PredictLocalActivation(runtime, power, now);

            var duringCooldown = machine.Evaluate(now + 12d, runtime);
            Assert.AreEqual(SpecialPowerState.Cooldown, duringCooldown);
            Assert.IsFalse(machine.CanActivate(duringCooldown));

            Object.DestroyImmediate(power);
        }
    }
}
