using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
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
