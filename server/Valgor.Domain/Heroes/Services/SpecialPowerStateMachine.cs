using Valgor.Domain.Heroes.Enums;

namespace Valgor.Domain.Heroes.Services;

public sealed record SpecialPowerRuntimeSnapshot(
    SpecialPowerState State,
    DateTime? ActiveUntilUtc,
    DateTime? CooldownUntilUtc);

public sealed class SpecialPowerStateMachine
{
    public SpecialPowerRuntimeSnapshot Evaluate(
        DateTime utcNow,
        DateTime? activeUntilUtc,
        DateTime? cooldownUntilUtc)
    {
        if (activeUntilUtc is not null && utcNow < activeUntilUtc.Value)
        {
            return new SpecialPowerRuntimeSnapshot(SpecialPowerState.Active, activeUntilUtc, cooldownUntilUtc);
        }

        if (cooldownUntilUtc is not null && utcNow < cooldownUntilUtc.Value)
        {
            return new SpecialPowerRuntimeSnapshot(SpecialPowerState.Cooldown, activeUntilUtc, cooldownUntilUtc);
        }

        return new SpecialPowerRuntimeSnapshot(SpecialPowerState.Ready, null, null);
    }

    public bool CanActivate(SpecialPowerRuntimeSnapshot snapshot) =>
        snapshot.State == SpecialPowerState.Ready;

    public SpecialPowerRuntimeSnapshot Activate(
        DateTime utcNow,
        float activeDurationSec,
        float cooldownSec)
    {
        if (activeDurationSec <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(activeDurationSec));
        }

        if (cooldownSec < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(cooldownSec));
        }

        var activeUntil = utcNow.AddSeconds(activeDurationSec);
        var cooldownUntil = utcNow.AddSeconds(cooldownSec);
        return new SpecialPowerRuntimeSnapshot(SpecialPowerState.Active, activeUntil, cooldownUntil);
    }
}
