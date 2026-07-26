using Valgor.Domain.Heroes;

namespace Valgor.Application.Common.Interfaces.Heroes;

public interface IPlayerHeroStore
{
    Task<IReadOnlyList<PlayerHero>> GetByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task EnsureRosterAsync(Guid playerId, IReadOnlyList<HeroDefinition> catalog, CancellationToken cancellationToken = default);
}
