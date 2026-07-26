using Valgor.Domain.Heroes;
using Valgor.Domain.Heroes.Enums;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Domain.Tests;

public sealed class SpecialPowerStateMachineTests
{
    private readonly SpecialPowerStateMachine _machine = new();

    [Fact]
    public void Cannot_Activate_During_Cooldown()
    {
        var now = DateTime.UtcNow;
        var activated = _machine.Activate(now, activeDurationSec: 8f, cooldownSec: 40f);
        var duringCooldown = _machine.Evaluate(now.AddSeconds(10), activated.ActiveUntilUtc, activated.CooldownUntilUtc);

        Assert.Equal(SpecialPowerState.Cooldown, duringCooldown.State);
        Assert.False(_machine.CanActivate(duringCooldown));
    }

    [Fact]
    public void Reconnection_Restores_Active_And_Cooldown_Timestamps()
    {
        var now = DateTime.UtcNow;
        var state = BattleHeroSpecialState.Create("battle-1", Guid.NewGuid(), "HERO_ELYRA_001");
        var snapshot = _machine.Activate(now, 10f, 35f);
        state.RestoreFromTimestamps(snapshot.ActiveUntilUtc, snapshot.CooldownUntilUtc, now);

        var restored = state.Snapshot(_machine, now.AddSeconds(2));
        Assert.Equal(SpecialPowerState.Active, restored.State);
        Assert.Equal(snapshot.ActiveUntilUtc, restored.ActiveUntilUtc);
        Assert.Equal(snapshot.CooldownUntilUtc, restored.CooldownUntilUtc);
    }

    [Fact]
    public void Duplicate_Activation_Is_Idempotent()
    {
        var playerId = Guid.NewGuid();
        var state = BattleHeroSpecialState.Create("battle-2", playerId, "HERO_VORTEX_000");
        var now = DateTime.UtcNow;

        Assert.True(state.TryActivate(_machine, now, 10f, 60f, "idem-1", out var first, out var firstDup));
        Assert.False(firstDup);
        Assert.Equal(SpecialPowerState.Active, first.State);

        Assert.True(state.TryActivate(_machine, now.AddSeconds(1), 10f, 60f, "idem-1", out var second, out var secondDup));
        Assert.True(secondDup);
        Assert.Equal(first.ActiveUntilUtc, second.ActiveUntilUtc);
        Assert.Equal(first.CooldownUntilUtc, second.CooldownUntilUtc);
    }

    [Fact]
    public void Dead_Hero_Cannot_Activate()
    {
        var state = BattleHeroSpecialState.Create("battle-3", Guid.NewGuid(), "HERO_AKEMI_005");
        var now = DateTime.UtcNow;
        state.MarkDead(now);

        Assert.False(state.TryActivate(_machine, now, 8f, 40f, "idem-dead", out _, out _));
    }
}
