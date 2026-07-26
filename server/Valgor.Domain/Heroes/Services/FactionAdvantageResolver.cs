namespace Valgor.Domain.Heroes.Services;

/// <summary>
/// Circular advantage: Rosa &gt; Guarda &gt; Asas &gt; Rosa.
/// Multiplier comes from configuration; never hardcode combat outcomes in the client.
/// </summary>
public sealed class FactionAdvantageResolver
{
    private readonly IReadOnlyDictionary<string, string> _beats;
    private readonly decimal _damageMultiplier;

    public FactionAdvantageResolver(
        IReadOnlyDictionary<string, string> beats,
        decimal damageMultiplier)
    {
        ArgumentNullException.ThrowIfNull(beats);
        if (damageMultiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(damageMultiplier));
        }

        _beats = beats;
        _damageMultiplier = damageMultiplier;
    }

    public bool HasAdvantage(string attackerFactionId, string defenderFactionId)
    {
        if (string.IsNullOrWhiteSpace(attackerFactionId) || string.IsNullOrWhiteSpace(defenderFactionId))
        {
            return false;
        }

        return _beats.TryGetValue(attackerFactionId, out var beaten)
               && string.Equals(beaten, defenderFactionId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns the damage multiplier for a single attack. Advantages do not stack.
    /// </summary>
    public decimal ResolveDamageMultiplier(string attackerFactionId, string defenderFactionId)
    {
        return HasAdvantage(attackerFactionId, defenderFactionId)
            ? _damageMultiplier
            : 1.0m;
    }
}
