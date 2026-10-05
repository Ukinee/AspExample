using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

public class CreateAnnouncementRequest
{
    public required string Contents { get; init; }
}

public class UpdateAnnouncementRequest
{
    public required string Contents { get; init; }
}

public class AnnouncementResponse
{
    public required AnnouncementIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public required string Contents { get; init; }
}

public class AnnouncementSignalRRequest
{
}

[Identifier(nameof(Guid), nameof(UserGuid))]
[RouteParamsIdentifier]
public readonly partial record struct AnnouncementIdentifier
{
    public required Guid UserGuid { get; init; }
    public required Guid Guid { get; init; }

    public static AnnouncementIdentifier Create(UserContext userContext, Guid guid)
    {
        return new AnnouncementIdentifier {
            UserGuid = userContext.Guid,
            Guid = guid,
        };
    }
}

[HasIdentifier(typeof(AnnouncementIdentifier))]
public partial record Announcement : IEntity<AnnouncementIdentifier>, IEntityWithPublicRead<Announcement>
{
    public required partial AnnouncementIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public required string Contents { get; init; }

    public Announcement OpenPublicRead()
    {
        throw new NotImplementedException();
    }

    public Announcement ClosePublicRead()
    {
        throw new NotImplementedException();
    }
}
