namespace Valgor.Domain.Heroes;

public sealed class FactionTeamBonus
{
    private FactionTeamBonus()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public int SameFactionCount { get; private set; }
    public int OtherFactionCount { get; private set; }
    public decimal TotalTroopAttackMultiplier { get; private set; }

    public static FactionTeamBonus Create(int sameFactionCount, int otherFactionCount, decimal totalTroopAttackMultiplier)
    {
        if (sameFactionCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sameFactionCount));
        }

        if (otherFactionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(otherFactionCount));
        }

        if (totalTroopAttackMultiplier <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTroopAttackMultiplier));
        }

        return new FactionTeamBonus
        {
            SameFactionCount = sameFactionCount,
            OtherFactionCount = otherFactionCount,
            TotalTroopAttackMultiplier = totalTroopAttackMultiplier
        };
    }
}
