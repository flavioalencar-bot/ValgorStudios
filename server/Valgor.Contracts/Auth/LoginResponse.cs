namespace Valgor.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    string Email,
    string DisplayName,
    string Role);
