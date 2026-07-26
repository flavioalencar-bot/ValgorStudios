using MediatR;
using Valgor.Application.Common.Interfaces;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Auth;

namespace Valgor.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUserStore userStore,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userStore.FindByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(Error.Unauthorized("Credenciais inválidas."));
        }

        var token = jwtTokenService.CreateToken(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.DisplayName);

        return Result.Success(new LoginResponse(
            token,
            "Bearer",
            user.Email,
            user.DisplayName,
            user.Role.ToString()));
    }
}
