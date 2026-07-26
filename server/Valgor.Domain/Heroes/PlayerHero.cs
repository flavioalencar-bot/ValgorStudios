using Valgor.Domain.Common;

namespace Valgor.Domain.Heroes;

public sealed class PlayerHero : BaseEntity
{
    private PlayerHero()
    {
    }

    public Guid PlayerId { get; private set; }
    public string HeroId { get; private set; } = string.Empty;
    public int Level { get; private set; } = 1;
    public int Stars { get; private set; }
    public int Fragments { get; private set; }
    public string ActiveSkinId { get; private set; } = string.Empty;
    public bool Unlocked { get; private set; }

    public static PlayerHero Create(Guid playerId, string heroId, string defaultSkinId, bool unlocked = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heroId);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultSkinId);

        return new PlayerHero
        {
            PlayerId = playerId,
            HeroId = heroId.Trim(),
            ActiveSkinId = defaultSkinId.Trim(),
            Unlocked = unlocked,
            Level = 1,
            Stars = unlocked ? 1 : 0,
            Fragments = 0,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
