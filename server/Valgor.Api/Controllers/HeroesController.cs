using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Valgor.Api.Extensions;
using Valgor.Application.Heroes.ActivateSpecial;
using Valgor.Application.Heroes.Catalog;
using Valgor.Application.Heroes.Factions;
using Valgor.Application.Heroes.PlayerHeroes;
using Valgor.Application.Heroes.TeamBonuses;
using Valgor.Application.Heroes.ValidateTeam;
using Valgor.Contracts.Heroes;

namespace Valgor.Api.Controllers;

[ApiController]
[Route("api/heroes")]
public sealed class HeroesController(IMediator mediator) : ControllerBase
{
    [HttpGet("catalog")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HeroCatalogResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetCatalog(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHeroCatalogQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    [HttpGet("{heroId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HeroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById(string heroId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHeroByIdQuery(heroId), cancellationToken);
        return result.ToHttpResult();
    }

    [HttpGet("factions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FactionsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetFactions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFactionsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    [HttpGet("team-bonuses")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TeamBonusesResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetTeamBonuses(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamBonusesQuery(), cancellationToken);
        return result.ToHttpResult();
    }
}

[ApiController]
[Route("api/players/me/heroes")]
public sealed class PlayerHeroesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PlayerHeroesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetMine(CancellationToken cancellationToken)
    {
        var playerId = ResolvePlayerId(User);
        var result = await mediator.Send(new GetMyHeroesQuery(playerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static Guid ResolvePlayerId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}

[ApiController]
[Route("api/teams")]
public sealed class TeamsController(IMediator mediator) : ControllerBase
{
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateTeamResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Validate([FromBody] ValidateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ValidateTeamCommand(request.HeroIds), cancellationToken);
        return result.ToHttpResult();
    }
}

[ApiController]
[Route("api/battle")]
public sealed class BattleHeroesController(IMediator mediator) : ControllerBase
{
    [HttpPost("{battleId}/heroes/{heroId}/special/activate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ActivateSpecialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ActivateSpecial(
        string battleId,
        string heroId,
        [FromBody] ActivateSpecialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ActivateSpecialCommand(battleId, heroId, request.PlayerId, request.IdempotencyKey),
            cancellationToken);
        return result.ToHttpResult();
    }
}
