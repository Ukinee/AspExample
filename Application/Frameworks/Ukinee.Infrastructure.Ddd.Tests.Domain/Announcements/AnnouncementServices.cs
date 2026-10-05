using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

public class AnnouncementFactory : ICreateEntityFactory<CreateAnnouncementRequest, Announcement>, IUpdateEntityFactory<UpdateAnnouncementRequest, Announcement>
{
    public Announcement Create(UserContext userContext, CreateAnnouncementRequest payload)
    {
        return new Announcement {
            Identifier = AnnouncementIdentifier.Create(userContext, Guid.NewGuid()),
            IsAvailableForPublicRead = true,
            Contents = payload.Contents,
        };
    }

    public Announcement Update(UserContext userContext, Announcement old, UpdateAnnouncementRequest payload)
    {
        return old with {
            Contents = payload.Contents,
        };
    }
}

public class AnnouncementMapService : IMapService<AnnouncementResponse, Announcement>
{
    public ValueTask<Announcement> Map(AnnouncementResponse externalEntity)
    {
        return ValueTask.FromResult(
            new Announcement {
                Identifier = externalEntity.Identifier,
                IsAvailableForPublicRead = externalEntity.IsAvailableForPublicRead,
                Contents = externalEntity.Contents,
            }
        );
    }

    public async ValueTask<IReadOnlyCollection<Announcement>> Map(IReadOnlyCollection<AnnouncementResponse> externalEntities)
    {
        var result = new List<Announcement>(externalEntities.Count);

        foreach (var externalEntity in externalEntities)
        {
            var entity = await Map(externalEntity);

            result.Add(entity);
        }

        return result;
    }
}

public class CreateAnnouncementRequestValidator : AbstractValidator<IEnumerable<CreateAnnouncementRequest>> { }
public class UpdateAnnouncementRequestValidator : AbstractValidator<IEnumerable<UpdateAnnouncementRequest>> { }
public class AnnouncementSignalRRouteResolver : RouteResolverBase<TestingTag, AnnouncementIdentifier, Announcement, AnnouncementSignalRRequest>;

public class AnnouncementSignalRAccessValidator : ISignalRAccessValidator<AnnouncementSignalRRequest>
{
    public Task<bool> ValidateAccess(AnnouncementSignalRRequest request, UserContext userContext)
    {
        throw new NotImplementedException();
    }
}
