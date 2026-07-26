namespace Valgor.Domain.Heroes;

public sealed class HeroSkin
{
    private HeroSkin()
    {
    }

    public string Id { get; private set; } = string.Empty;
    public string HeroId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Rarity { get; private set; } = string.Empty;
    public string ModelAddress { get; private set; } = string.Empty;
    public string MaterialSetAddress { get; private set; } = string.Empty;
    public string PortraitAddress { get; private set; } = string.Empty;
    public bool CompetitiveNormalization { get; private set; } = true;
    public bool IsDefault { get; private set; }

    public static HeroSkin CreateDefault(string heroId, string heroDisplayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);
        var id = $"SKIN_{heroId}_DEFAULT";
        return new HeroSkin
        {
            Id = id,
            HeroId = heroId.Trim(),
            Name = $"{heroDisplayName} — Padrão",
            Rarity = "Comum",
            ModelAddress = $"heroes/{heroId}/skins/default/model",
            MaterialSetAddress = $"heroes/{heroId}/skins/default/materials",
            PortraitAddress = $"heroes/{heroId}/portrait",
            CompetitiveNormalization = true,
            IsDefault = true
        };
    }

    public static HeroSkin Create(
        string id,
        string heroId,
        string name,
        string rarity,
        string modelAddress,
        bool competitiveNormalization = true,
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);

        return new HeroSkin
        {
            Id = id.Trim(),
            HeroId = heroId.Trim(),
            Name = name.Trim(),
            Rarity = rarity.Trim(),
            ModelAddress = modelAddress.Trim(),
            MaterialSetAddress = $"{modelAddress.Trim()}/materials",
            PortraitAddress = $"heroes/{heroId}/skins/{id}/portrait",
            CompetitiveNormalization = competitiveNormalization,
            IsDefault = isDefault
        };
    }
}
