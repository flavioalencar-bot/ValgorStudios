namespace Valgor.Domain.Heroes;

public sealed class FactionAdvantage
{
    private FactionAdvantage()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string AttackerFactionId { get; private set; } = string.Empty;
    public string DefenderFactionId { get; private set; } = string.Empty;
    public decimal DamageMultiplier { get; private set; }

    public static FactionAdvantage Create(string attackerFactionId, string defenderFactionId, decimal damageMultiplier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attackerFactionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(defenderFactionId);
        if (damageMultiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(damageMultiplier));
        }

        return new FactionAdvantage
        {
            AttackerFactionId = attackerFactionId.Trim(),
            DefenderFactionId = defenderFactionId.Trim(),
            DamageMultiplier = damageMultiplier
        };
    }
}
