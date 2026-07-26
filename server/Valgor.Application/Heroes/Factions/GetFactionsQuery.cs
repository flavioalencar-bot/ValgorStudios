using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;

namespace Valgor.Application.Heroes.Factions;

public sealed record GetFactionsQuery : IRequest<Result<FactionsResponse>>;

public sealed class GetFactionsQueryHandler(IHeroCatalogStore catalogStore)
    : IRequestHandler<GetFactionsQuery, Result<FactionsResponse>>
{
    public async Task<Result<FactionsResponse>> Handle(GetFactionsQuery request, CancellationToken cancellationToken)
    {
        var factions = await catalogStore.GetFactionsAsync(cancellationToken);
        var multiplier = await catalogStore.GetAdvantageDamageMultiplierAsync(cancellationToken);
        var dto = factions.Select(f => new FactionDto(f.Id, f.Color, f.Archetype, f.BeatsFactionId, f.LosesToFactionId)).ToArray();
        return Result.Success(new FactionsResponse(dto, multiplier));
    }
}
