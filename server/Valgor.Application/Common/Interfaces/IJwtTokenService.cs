namespace Valgor.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, string email, string role, string displayName);
}
