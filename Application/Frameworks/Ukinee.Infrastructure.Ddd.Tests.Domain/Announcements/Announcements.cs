using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

public class CreateAnnouncementRequest
{
    public required string Contents { get; init; }
}

public class UpdateAnnouncementContentsRequest
{
    public required string Contents { get; init; }
}

public class AnnouncementResponse
{
    public required AnnouncementIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public required string Contents { get; init; }
}

public class AnnouncementSignalRRequest : ISignalRSubscribeRequest
{
    public Guid Identifier { get; init; }
    public SubscribeAccessModes AccessModes { get; init; }
}

[ApiIdentifier]
[Identifier(nameof(UserGuid), nameof(Guid))]
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
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public Announcement ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
