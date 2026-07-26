using Microsoft.EntityFrameworkCore;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Domain.Heroes;
using Valgor.Infrastructure.Persistence;

namespace Valgor.Infrastructure.Heroes;

public sealed class PlayerHeroStore(ValgorDbContext dbContext) : IPlayerHeroStore
{
    public async Task<IReadOnlyList<PlayerHero>> GetByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default) =>
        await dbContext.PlayerHeroes.AsNoTracking()
            .Where(h => h.PlayerId == playerId)
            .OrderBy(h => h.HeroId)
            .ToListAsync(cancellationToken);

    public async Task EnsureRosterAsync(Guid playerId, IReadOnlyList<HeroDefinition> catalog, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.PlayerHeroes
            .Where(h => h.PlayerId == playerId)
            .Select(h => h.HeroId)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet(StringComparer.Ordinal);
        foreach (var hero in catalog)
        {
            if (existingSet.Contains(hero.Id))
            {
                continue;
            }

            await dbContext.PlayerHeroes.AddAsync(
                PlayerHero.Create(playerId, hero.Id, hero.DefaultSkinId, unlocked: true),
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
