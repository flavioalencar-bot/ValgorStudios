using System.Text.Json;
using System.Text.Json.Serialization;
using Valgor.Domain.Heroes;

namespace Valgor.Infrastructure.Persistence.Seed;

internal sealed class HeroesSeedDocument
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("factions")]
    public Dictionary<string, FactionSeed> Factions { get; set; } = new();

    [JsonPropertyName("teamBonuses")]
    public List<TeamBonusSeed> TeamBonuses { get; set; } = [];

    [JsonPropertyName("heroes")]
    public List<HeroSeed> Heroes { get; set; } = [];
}

internal sealed class FactionSeed
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("archetype")]
    public string Archetype { get; set; } = string.Empty;

    [JsonPropertyName("beats")]
    public string Beats { get; set; } = string.Empty;

    [JsonPropertyName("losesTo")]
    public string LosesTo { get; set; } = string.Empty;
}

internal sealed class TeamBonusSeed
{
    [JsonPropertyName("sameFaction")]
    public int SameFaction { get; set; }

    [JsonPropertyName("otherFaction")]
    public int OtherFaction { get; set; }

    [JsonPropertyName("totalTroopAttackMultiplier")]
    public decimal TotalTroopAttackMultiplier { get; set; }
}

internal sealed class HeroSeed
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; } = string.Empty;

    [JsonPropertyName("faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("class")]
    public string Class { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("weapon")]
    public string Weapon { get; set; } = string.Empty;

    [JsonPropertyName("element")]
    public string Element { get; set; } = string.Empty;

    [JsonPropertyName("power")]
    public PowerSeed Power { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}

internal sealed class PowerSeed
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("durationSec")]
    public float DurationSec { get; set; }

    [JsonPropertyName("cooldownSec")]
    public float CooldownSec { get; set; }

    [JsonPropertyName("effects")]
    public List<string> Effects { get; set; } = [];
}

public static class HeroesSeedLoader
{
    /// <summary>MVP circular advantage multiplier from VALGOR_HEROES_MASTER.md.</summary>
    public const decimal DefaultAdvantageDamageMultiplier = 1.15m;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static HeroesSeedBundle LoadFromEmbeddedOrFile()
    {
        var path = ResolveSeedPath();
        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<HeroesSeedDocument>(json, JsonOptions)
                       ?? throw new InvalidOperationException("heroes.seed.json inválido.");

        Validate(document);
        return Map(document);
    }

    public static string ResolveSeedPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Seed", "heroes.seed.json"),
            Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "heroes.seed.json"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Valgor.Infrastructure", "Persistence", "Seed", "heroes.seed.json")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "game-design", "heroes", "heroes.seed.json")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Persistence", "Seed", "heroes.seed.json")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Valgor.Infrastructure", "Persistence", "Seed", "heroes.seed.json")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs", "game-design", "heroes", "heroes.seed.json"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("heroes.seed.json não encontrado.", string.Join(" | ", candidates));
    }

    private static void Validate(HeroesSeedDocument document)
    {
        if (document.Heroes.Count != 11)
        {
            throw new InvalidOperationException($"Seed deve conter 11 heróis; encontrado {document.Heroes.Count}.");
        }

        if (document.Factions.Count != 3)
        {
            throw new InvalidOperationException("Seed deve conter exatamente 3 facções.");
        }

        foreach (var required in FactionIds.All)
        {
            if (!document.Factions.ContainsKey(required))
            {
                throw new InvalidOperationException($"Facção obrigatória ausente: {required}");
            }
        }

        var ids = document.Heroes.Select(h => h.Id).ToHashSet(StringComparer.Ordinal);
        if (ids.Count != 11)
        {
            throw new InvalidOperationException("IDs de heróis duplicados no seed.");
        }
    }

    private static HeroesSeedBundle Map(HeroesSeedDocument document)
    {
        var factions = document.Factions.Select(pair =>
            HeroFaction.Create(pair.Key, pair.Value.Color, pair.Value.Archetype, pair.Value.Beats, pair.Value.LosesTo)).ToList();

        var advantages = factions
            .Select(f => FactionAdvantage.Create(f.Id, f.BeatsFactionId, DefaultAdvantageDamageMultiplier))
            .ToList();

        var bonuses = document.TeamBonuses
            .Select(b => FactionTeamBonus.Create(b.SameFaction, b.OtherFaction, b.TotalTroopAttackMultiplier))
            .ToList();

        var heroes = new List<HeroDefinition>();
        foreach (var seed in document.Heroes)
        {
            var hero = HeroDefinition.Create(
                seed.Id,
                seed.Name,
                seed.Title,
                seed.Gender,
                seed.Rarity,
                seed.Faction,
                seed.Class,
                seed.Role,
                seed.Position,
                seed.Weapon,
                seed.Element,
                seed.Status,
                seed.Notes);

            var power = HeroSpecialPower.Create(seed.Id, seed.Power.Name, seed.Power.DurationSec, seed.Power.CooldownSec);
            hero.SetSpecialPower(power);

            for (var i = 0; i < seed.Power.Effects.Count; i++)
            {
                hero.AddEffect(HeroSpecialEffect.Create(seed.Id, power.Id, i, seed.Power.Effects[i]));
            }

            hero.AddSkin(HeroSkin.CreateDefault(seed.Id, hero.DisplayName));

            if (string.Equals(seed.Id, "HERO_CONSORTE_002", StringComparison.Ordinal))
            {
                hero.AddSkin(HeroSkin.Create(
                    "SKIN_HERO_CONSORTE_002_ROYAL",
                    seed.Id,
                    "Skin Real",
                    "Épica",
                    "heroes/HERO_CONSORTE_002/skins/royal/model"));
            }

            heroes.Add(hero);
        }

        return new HeroesSeedBundle(document.Version, factions, advantages, bonuses, heroes);
    }
}

public sealed record HeroesSeedBundle(
    string Version,
    IReadOnlyList<HeroFaction> Factions,
    IReadOnlyList<FactionAdvantage> Advantages,
    IReadOnlyList<FactionTeamBonus> TeamBonuses,
    IReadOnlyList<HeroDefinition> Heroes);
