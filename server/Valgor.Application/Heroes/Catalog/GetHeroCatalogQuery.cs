using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;

namespace Valgor.Application.Heroes.Catalog;

public sealed record GetHeroCatalogQuery : IRequest<Result<HeroCatalogResponse>>;

public sealed class GetHeroCatalogQueryHandler(IHeroCatalogStore catalogStore)
    : IRequestHandler<GetHeroCatalogQuery, Result<HeroCatalogResponse>>
{
    public async Task<Result<HeroCatalogResponse>> Handle(GetHeroCatalogQuery request, CancellationToken cancellationToken)
    {
        var heroes = await catalogStore.GetAllAsync(cancellationToken);
        var dto = heroes.Select(HeroMapping.ToDto).ToArray();
        return Result.Success(new HeroCatalogResponse("1.0.0", dto));
    }
}
