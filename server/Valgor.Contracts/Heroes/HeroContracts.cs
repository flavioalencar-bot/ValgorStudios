namespace Valgor.Contracts.Heroes;

public sealed record HeroCatalogResponse(string Version, IReadOnlyList<HeroDto> Heroes);

public sealed record HeroDto(
    string Id,
    string Name,
    string Title,
    string DisplayName,
    string Gender,
    string Rarity,
    string Faction,
    string Class,
    string Role,
    string Position,
    string Weapon,
    string Element,
    string Status,
    string Notes,
    string DefaultSkinId,
    string PrefabAddress,
    string PortraitAddress,
    SpecialPowerDto? SpecialPower);

public sealed record SpecialPowerDto(
    string Id,
    string Name,
    float ActiveDurationSec,
    float CooldownSec,
    string TargetType,
    bool Interruptible,
    bool CanActivateWhileControlled,
    string AnimationState,
    string VfxAddress,
    string SfxAddress,
    IReadOnlyList<string> Effects);

public sealed record FactionDto(
    string Id,
    string Color,
    string Archetype,
    string Beats,
    string LosesTo);

public sealed record FactionsResponse(IReadOnlyList<FactionDto> Factions, decimal AdvantageDamageMultiplier);

public sealed record TeamBonusDto(int SameFaction, int OtherFaction, decimal TotalTroopAttackMultiplier);

public sealed record TeamBonusesResponse(IReadOnlyList<TeamBonusDto> Bonuses);

public sealed record PlayerHeroDto(
    string HeroId,
    string DisplayName,
    string Faction,
    int Level,
    int Stars,
    int Fragments,
    string ActiveSkinId,
    bool Unlocked);

public sealed record PlayerHeroesResponse(IReadOnlyList<PlayerHeroDto> Heroes);

public sealed record ValidateTeamRequest(IReadOnlyList<string> HeroIds);

public sealed record ValidateTeamResponse(
    bool IsValid,
    string? Error,
    decimal TroopAttackMultiplier,
    int SameFactionCount,
    string? DominantFactionId,
    IReadOnlyList<string> HeroIds,
    IReadOnlyList<string> FactionIds);

public sealed record ActivateSpecialRequest(Guid PlayerId, string IdempotencyKey);

public sealed record ActivateSpecialResponse(
    string BattleId,
    string HeroId,
    string State,
    DateTime? ActiveUntilUtc,
    DateTime? CooldownUntilUtc,
    bool IdempotentReplay);

public sealed record HeroSkinDto(
    string Id,
    string HeroId,
    string Name,
    string Rarity,
    string ModelAddress,
    string PortraitAddress,
    bool CompetitiveNormalization,
    bool IsDefault);
