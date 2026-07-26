using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;

namespace Valgor.Application.Heroes.Catalog;

public sealed record GetHeroByIdQuery(string HeroId) : IRequest<Result<HeroDto>>;

public sealed class GetHeroByIdQueryHandler(IHeroCatalogStore catalogStore)
    : IRequestHandler<GetHeroByIdQuery, Result<HeroDto>>
{
    public async Task<Result<HeroDto>> Handle(GetHeroByIdQuery request, CancellationToken cancellationToken)
    {
        var hero = await catalogStore.GetByIdAsync(request.HeroId, cancellationToken);
        if (hero is null)
        {
            return Result.Failure<HeroDto>(Error.NotFound($"Herói '{request.HeroId}' não encontrado."));
        }

        return Result.Success(HeroMapping.ToDto(hero));
    }
}
