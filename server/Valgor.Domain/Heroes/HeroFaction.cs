namespace Valgor.Domain.Heroes;

public sealed class HeroFaction
{
    private HeroFaction()
    {
    }

    public string Id { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public string Archetype { get; private set; } = string.Empty;
    public string BeatsFactionId { get; private set; } = string.Empty;
    public string LosesToFactionId { get; private set; } = string.Empty;

    public static HeroFaction Create(
        string id,
        string color,
        string archetype,
        string beatsFactionId,
        string losesToFactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return new HeroFaction
        {
            Id = id.Trim(),
            Color = color.Trim(),
            Archetype = archetype.Trim(),
            BeatsFactionId = beatsFactionId.Trim(),
            LosesToFactionId = losesToFactionId.Trim()
        };
    }
}
