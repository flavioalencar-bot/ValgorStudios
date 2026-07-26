namespace Valgor.Domain.Heroes;

public sealed class HeroSpecialEffect
{
    private HeroSpecialEffect()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string HeroId { get; private set; } = string.Empty;
    public string SpecialPowerId { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public static HeroSpecialEffect Create(string heroId, string specialPowerId, int sortOrder, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);
        ArgumentException.ThrowIfNullOrWhiteSpace(specialPowerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new HeroSpecialEffect
        {
            HeroId = heroId.Trim(),
            SpecialPowerId = specialPowerId.Trim(),
            SortOrder = sortOrder,
            Description = description.Trim()
        };
    }
}
