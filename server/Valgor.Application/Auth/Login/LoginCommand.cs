using MediatR;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Auth;

namespace Valgor.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
