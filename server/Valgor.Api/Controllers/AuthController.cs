using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Valgor.Api.Extensions;
using Valgor.Application.Auth.Login;
using Valgor.Contracts.Auth;

namespace Valgor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return result.ToHttpResult();
    }
}
