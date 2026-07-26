namespace Valgor.Domain.Heroes.Services;

public sealed record TeamBonusRule(int SameFaction, int OtherFaction, decimal TotalTroopAttackMultiplier);

public sealed record TeamBonusResult(decimal TotalTroopAttackMultiplier, int SameFactionCount, string? DominantFactionId);

/// <summary>
/// Composition bonuses approved in the heroes bible / seed.
/// Matching prefers the highest applicable multiplier.
/// </summary>
public sealed class FactionBonusCalculator
{
    private readonly IReadOnlyList<TeamBonusRule> _rules;

    public FactionBonusCalculator(IReadOnlyList<TeamBonusRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules
            .OrderByDescending(rule => rule.TotalTroopAttackMultiplier)
            .ToArray();
    }

    public TeamBonusResult Calculate(IReadOnlyList<string> factionIds)
    {
        ArgumentNullException.ThrowIfNull(factionIds);

        if (factionIds.Count == 0)
        {
            return new TeamBonusResult(1.0m, 0, null);
        }

        var counts = factionIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id, StringComparer.Ordinal)
            .Select(group => new { FactionId = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ToArray();

        var dominant = counts[0];
        var same = dominant.Count;
        var other = factionIds.Count - same;

        foreach (var rule in _rules)
        {
            if (rule.OtherFaction == 0)
            {
                if (same >= rule.SameFaction && other == 0)
                {
                    return new TeamBonusResult(rule.TotalTroopAttackMultiplier, same, dominant.FactionId);
                }
            }
            else if (same >= rule.SameFaction && other == rule.OtherFaction)
            {
                return new TeamBonusResult(rule.TotalTroopAttackMultiplier, same, dominant.FactionId);
            }
        }

        return new TeamBonusResult(1.0m, same, dominant.FactionId);
    }
}
