namespace Ukinee.Users.Domain;

public class TokenOptions
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    public required int AccessTokenLifetimeMinutes { get; init; }
    public required int ClockSkewSeconds { get; init; }

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(AccessTokenLifetimeMinutes);
    public TimeSpan ClockSkew => TimeSpan.FromSeconds(ClockSkewSeconds);
}
