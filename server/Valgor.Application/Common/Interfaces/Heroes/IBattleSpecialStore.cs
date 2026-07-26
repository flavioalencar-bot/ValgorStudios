using Valgor.Domain.Heroes;

namespace Valgor.Application.Common.Interfaces.Heroes;

public interface IBattleSpecialStore
{
    Task<BattleHeroSpecialState?> GetAsync(string battleId, Guid playerId, string heroId, CancellationToken cancellationToken = default);
    Task<BattleHeroSpecialState> GetOrCreateAsync(string battleId, Guid playerId, string heroId, CancellationToken cancellationToken = default);
    Task SaveAsync(BattleHeroSpecialState state, CancellationToken cancellationToken = default);
}
