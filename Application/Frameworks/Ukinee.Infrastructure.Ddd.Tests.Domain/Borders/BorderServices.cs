using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

public class BorderFactory : ICreateEntityFactory<CreateBorderRequest, Border>, IUpdateEntityFactory<UpdateBorderRequest, Border>
{
    public Border Create(UserContext userContext, CreateBorderRequest payload)
    {
        return new Border {
            Identifier = BorderIdentifier.Create(Guid.NewGuid()),
            Size = payload.Size,
            IsAvailableForPublicRead = true,
        };
    }

    public Border Update(UserContext userContext, Border old, UpdateBorderRequest payload)
    {
        return old with {
            Size = payload.Size,
        };
    }
}

public class BorderMapService : IMapService<BorderResponse, Border>
{
    public ValueTask<Border> Map(BorderResponse externalEntity)
    {
        return ValueTask.FromResult(
            new Border {
                Identifier = externalEntity.Identifier,
                IsAvailableForPublicRead = externalEntity.IsAvailableForPublicRead,
                Size = externalEntity.Size,
            }
        );
    }

    public async ValueTask<IReadOnlyCollection<Border>> Map(IReadOnlyCollection<BorderResponse> externalEntities)
    {
        var result = new List<Border>(externalEntities.Count);

        foreach (var externalEntity in externalEntities)
        {
            var entity = await Map(externalEntity);

            result.Add(entity);
        }

        return result;
    }
}


public class CreateBorderRequestValidator : AbstractValidator<IEnumerable<CreateBorderRequest>>;
public class UpdateBorderRequestValidator : AbstractValidator<IEnumerable<UpdateBorderRequest>>;
public class BorderSignalRRouteResolver : RouteResolverBase<TestingTag, BorderIdentifier, Border, BorderSignalRRequest>;

public class BorderSignalRAccessValidator : ISignalRAccessValidator<BorderSignalRRequest>
{
    public Task<bool> ValidateAccess(BorderSignalRRequest request, UserContext userContext)
    {
        throw new NotImplementedException();
    }
}
