using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;

namespace Valgor.Application.Heroes.TeamBonuses;

public sealed record GetTeamBonusesQuery : IRequest<Result<TeamBonusesResponse>>;

public sealed class GetTeamBonusesQueryHandler(IHeroCatalogStore catalogStore)
    : IRequestHandler<GetTeamBonusesQuery, Result<TeamBonusesResponse>>
{
    public async Task<Result<TeamBonusesResponse>> Handle(GetTeamBonusesQuery request, CancellationToken cancellationToken)
    {
        var bonuses = await catalogStore.GetTeamBonusesAsync(cancellationToken);
        var dto = bonuses
            .Select(b => new TeamBonusDto(b.SameFactionCount, b.OtherFactionCount, b.TotalTroopAttackMultiplier))
            .ToArray();
        return Result.Success(new TeamBonusesResponse(dto));
    }
}
