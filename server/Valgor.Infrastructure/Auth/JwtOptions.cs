namespace Valgor.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "valgor";
    public string Audience { get; set; } = "valgor-clients";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
