namespace Valgor.Contracts.Versioning;

public sealed record VersionResponse(
    string Version,
    string Product,
    string Environment,
    DateTime ServerTimeUtc);
