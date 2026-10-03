using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

[Identifier(nameof(UserGuid), nameof(Hash))]
[RouteParamsIdentifier(nameof(UserGuid))]
public readonly partial record struct CookieIdentifier
{
    public required string Hash { get; init; }

    public required Guid UserGuid { get; init; }

    public static CookieIdentifier Create(UserContext userContext, string hash)
    {
        return Create(userContext.Guid, hash);
    }
}

public record CreateCookieRequest
{
    public required int Calories { get; init; }
}

public record UpdateCookieRequest
{
    public required int Calories { get; init; }
}

public record CookieResponse
{
    public required CookieIdentifier Identifier { get; init; }

    public required int Calories { get; init; }
}

public record CookieSignalRRequest { }

[HasIdentifier(typeof(CookieIdentifier))]
public partial record Cookie : IEntity<CookieIdentifier>, IEntityWithPublicRead<Cookie>
{
    public required partial CookieIdentifier Identifier { get; init; }

    public required int Calories { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public Cookie OpenPublicRead()
    {
        throw new NotImplementedException();
    }

    public Cookie ClosePublicRead()
    {
        throw new NotImplementedException();
    }
}
