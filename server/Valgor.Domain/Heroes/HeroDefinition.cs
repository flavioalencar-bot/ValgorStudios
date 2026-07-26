namespace Valgor.Domain.Heroes;

public sealed class HeroDefinition
{
    private readonly List<HeroSpecialEffect> _effects = [];
    private readonly List<HeroSkin> _skins = [];

    private HeroDefinition()
    {
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Gender { get; private set; } = string.Empty;
    public string Rarity { get; private set; } = string.Empty;
    public string FactionId { get; private set; } = string.Empty;
    public string ClassName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public string Weapon { get; private set; } = string.Empty;
    public string Element { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string DefaultSkinId { get; private set; } = string.Empty;
    public string PrefabAddress { get; private set; } = string.Empty;
    public string PortraitAddress { get; private set; } = string.Empty;

    public HeroSpecialPower? SpecialPower { get; private set; }
    public IReadOnlyCollection<HeroSpecialEffect> Effects => _effects.AsReadOnly();
    public IReadOnlyCollection<HeroSkin> Skins => _skins.AsReadOnly();

    /// <summary>
    /// Pending civil names stay as temporary title while internal IDs remain the primary key.
    /// </summary>
    public string DisplayName =>
        string.Equals(Name, "A definir", StringComparison.OrdinalIgnoreCase)
            ? Title
            : Name;

    public static HeroDefinition Create(
        string id,
        string name,
        string title,
        string gender,
        string rarity,
        string factionId,
        string className,
        string role,
        string position,
        string weapon,
        string element,
        string status,
        string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return new HeroDefinition
        {
            Id = id.Trim(),
            Name = name.Trim(),
            Title = title.Trim(),
            Gender = gender.Trim(),
            Rarity = rarity.Trim(),
            FactionId = factionId.Trim(),
            ClassName = className.Trim(),
            Role = role.Trim(),
            Position = position.Trim(),
            Weapon = weapon.Trim(),
            Element = element.Trim(),
            Status = status.Trim(),
            Notes = notes.Trim(),
            DefaultSkinId = $"SKIN_{id.Trim()}_DEFAULT",
            PrefabAddress = $"heroes/{id.Trim()}/prefab",
            PortraitAddress = $"heroes/{id.Trim()}/portrait"
        };
    }

    public void SetSpecialPower(HeroSpecialPower power)
    {
        ArgumentNullException.ThrowIfNull(power);
        SpecialPower = power;
    }

    public void AddEffect(HeroSpecialEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        _effects.Add(effect);
    }

    public void AddSkin(HeroSkin skin)
    {
        ArgumentNullException.ThrowIfNull(skin);
        _skins.Add(skin);
    }
}
