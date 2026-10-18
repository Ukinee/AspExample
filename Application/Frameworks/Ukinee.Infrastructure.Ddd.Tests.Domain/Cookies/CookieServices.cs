using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

public class CookieFactory : ICreateEntityFactory<CreateCookieRequest, Cookie>, IUpdateEntityFactory<UpdateCookieRequest, Cookie>
{
    public Cookie Create(UserContext userContext, CreateCookieRequest payload)
    {
        return new Cookie {
            Identifier = CookieIdentifier.Create(userContext, Guid.NewGuid().ToString()),
            Calories = payload.Calories,
            IsAvailableForPublicRead = true,
        };
    }

    public Cookie Update(UserContext userContext, Cookie old, UpdateCookieRequest payload)
    {
        return old with {
            Calories = payload.Calories,
        };
    }
}

public class CookieMapService : IMapService<CookieResponse, Cookie>
{
    public ValueTask<Cookie> Map(CookieResponse externalEntity)
    {
        return ValueTask.FromResult(
            new Cookie {
                Identifier = externalEntity.Identifier,
                Calories = externalEntity.Calories,
                IsAvailableForPublicRead = externalEntity.IsAvailableForPublicRead,
            }
        );
    }

    public async ValueTask<IReadOnlyCollection<Cookie>> Map(IReadOnlyCollection<CookieResponse> externalEntities)
    {
        var result = new List<Cookie>(externalEntities.Count);

        foreach (var externalEntity in externalEntities)
        {
            var entity = await Map(externalEntity);

            result.Add(entity);
        }

        return result;
    }
}


public class CreateCookieRequestValidator : AbstractValidator<IEnumerable<CreateCookieRequest>>;
public class UpdateCookieRequestValidator : AbstractValidator<IEnumerable<UpdateCookieRequest>>;
public class CookieSignalRRouteResolver : RouteResolverBase<TestingTag, CookieIdentifier, Cookie, CookieSignalRRequest>;

public class CookieSignalRAccessValidator : ISignalRAccessValidator<CookieSignalRRequest>
{
    public Task<bool> ValidateAccess(CookieSignalRRequest request, UserContext userContext)
    {
        throw new NotImplementedException();
    }
}
