using Valgor.Domain.Common;
using Valgor.Domain.Heroes.Enums;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Domain.Heroes;

/// <summary>
/// Authoritative special-power runtime for a hero inside a battle session.
/// </summary>
public sealed class BattleHeroSpecialState : BaseEntity
{
    private BattleHeroSpecialState()
    {
    }

    public string BattleId { get; private set; } = string.Empty;
    public Guid PlayerId { get; private set; }
    public string HeroId { get; private set; } = string.Empty;
    public bool IsAlive { get; private set; } = true;
    public DateTime? ActiveUntilUtc { get; private set; }
    public DateTime? CooldownUntilUtc { get; private set; }
    public string? LastIdempotencyKey { get; private set; }

    public static BattleHeroSpecialState Create(string battleId, Guid playerId, string heroId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(battleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);

        return new BattleHeroSpecialState
        {
            BattleId = battleId.Trim(),
            PlayerId = playerId,
            HeroId = heroId.Trim(),
            IsAlive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public SpecialPowerRuntimeSnapshot Snapshot(SpecialPowerStateMachine machine, DateTime utcNow) =>
        machine.Evaluate(utcNow, ActiveUntilUtc, CooldownUntilUtc);

    public void MarkDead(DateTime utcNow)
    {
        IsAlive = false;
        MarkUpdated(utcNow);
    }

    public void RestoreFromTimestamps(DateTime? activeUntilUtc, DateTime? cooldownUntilUtc, DateTime utcNow)
    {
        ActiveUntilUtc = activeUntilUtc;
        CooldownUntilUtc = cooldownUntilUtc;
        MarkUpdated(utcNow);
    }

    public bool TryActivate(
        SpecialPowerStateMachine machine,
        DateTime utcNow,
        float activeDurationSec,
        float cooldownSec,
        string idempotencyKey,
        out SpecialPowerRuntimeSnapshot snapshot,
        out bool wasDuplicate)
    {
        wasDuplicate = false;
        snapshot = Snapshot(machine, utcNow);

        if (!string.IsNullOrWhiteSpace(idempotencyKey)
            && string.Equals(LastIdempotencyKey, idempotencyKey, StringComparison.Ordinal))
        {
            wasDuplicate = true;
            return true;
        }

        if (!IsAlive)
        {
            return false;
        }

        if (!machine.CanActivate(snapshot))
        {
            return false;
        }

        snapshot = machine.Activate(utcNow, activeDurationSec, cooldownSec);
        ActiveUntilUtc = snapshot.ActiveUntilUtc;
        CooldownUntilUtc = snapshot.CooldownUntilUtc;
        LastIdempotencyKey = idempotencyKey;
        MarkUpdated(utcNow);
        return true;
    }
}
