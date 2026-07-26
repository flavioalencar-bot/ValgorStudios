using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;

namespace Valgor.Application.Heroes.PlayerHeroes;

public sealed record GetMyHeroesQuery(Guid PlayerId) : IRequest<Result<PlayerHeroesResponse>>;

public sealed class GetMyHeroesQueryHandler(IHeroCatalogStore catalogStore, IPlayerHeroStore playerHeroStore)
    : IRequestHandler<GetMyHeroesQuery, Result<PlayerHeroesResponse>>
{
    public async Task<Result<PlayerHeroesResponse>> Handle(GetMyHeroesQuery request, CancellationToken cancellationToken)
    {
        if (request.PlayerId == Guid.Empty)
        {
            return Result.Failure<PlayerHeroesResponse>(Error.Unauthorized("Jogador não autenticado."));
        }

        var catalog = await catalogStore.GetAllAsync(cancellationToken);
        await playerHeroStore.EnsureRosterAsync(request.PlayerId, catalog, cancellationToken);
        var roster = await playerHeroStore.GetByPlayerAsync(request.PlayerId, cancellationToken);
        var byId = catalog.ToDictionary(h => h.Id, StringComparer.Ordinal);

        var dto = roster.Select(entry =>
        {
            byId.TryGetValue(entry.HeroId, out var definition);
            return new PlayerHeroDto(
                entry.HeroId,
                definition?.DisplayName ?? entry.HeroId,
                definition?.FactionId ?? string.Empty,
                entry.Level,
                entry.Stars,
                entry.Fragments,
                entry.ActiveSkinId,
                entry.Unlocked);
        }).ToArray();

        return Result.Success(new PlayerHeroesResponse(dto));
    }
}
