using Microsoft.EntityFrameworkCore;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Domain.Heroes;
using Valgor.Infrastructure.Persistence;

namespace Valgor.Infrastructure.Heroes;

public sealed class BattleSpecialStore(ValgorDbContext dbContext) : IBattleSpecialStore
{
    public async Task<BattleHeroSpecialState?> GetAsync(
        string battleId,
        Guid playerId,
        string heroId,
        CancellationToken cancellationToken = default) =>
        await dbContext.BattleHeroSpecialStates
            .FirstOrDefaultAsync(
                s => s.BattleId == battleId && s.PlayerId == playerId && s.HeroId == heroId,
                cancellationToken);

    public async Task<BattleHeroSpecialState> GetOrCreateAsync(
        string battleId,
        Guid playerId,
        string heroId,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(battleId, playerId, heroId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = BattleHeroSpecialState.Create(battleId, playerId, heroId);
        await dbContext.BattleHeroSpecialStates.AddAsync(created, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    public async Task SaveAsync(BattleHeroSpecialState state, CancellationToken cancellationToken = default)
    {
        dbContext.BattleHeroSpecialStates.Update(state);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
