using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Common.Domain.Models.Identifiers;

[Identifier(nameof(UserGuid))]
public readonly partial record struct UserIdentifier
{
    public required Guid UserGuid { get; init; }

    public static UserIdentifier New()
    {
        return new UserIdentifier() {
            UserGuid = Guid.NewGuid(),
        };
    }
}

[RouteParamsIdentifier(nameof(AttendeeUserGuid))]
[Identifier(nameof(EventGuid), nameof(EventUserGuid), nameof(AttendeeUserGuid))]
public readonly partial record struct AttendanceIdentifier
{
    public required Guid AttendeeUserGuid { get; init; }

    public required Guid EventGuid { get; init; }
    public required Guid EventUserGuid { get; init; }

    public static AttendanceIdentifier New(UserContext ticketUserContext, EventIdentifier eventIdentifier) =>
        Create(eventIdentifier.EventGuid, eventIdentifier.UserGuid, ticketUserContext.Guid);
}

[RouteParamsIdentifier(nameof(ReviewUserGuid))]
[Identifier(nameof(EventGuid), nameof(EventUserGuid), nameof(ReviewUserGuid), nameof(ReviewGuid))]
public readonly partial record struct ReviewIdentifier
{
    public required Guid EventGuid { get; init; }
    public required Guid EventUserGuid { get; init; }

    public required Guid ReviewUserGuid { get; init; }
    public required Guid ReviewGuid { get; init; }

    public static ReviewIdentifier New(UserContext reviewUserContext, EventIdentifier eventIdentifier) =>
        Create(eventIdentifier.EventGuid, eventIdentifier.UserGuid, reviewUserContext.Guid, Guid.NewGuid());
}

[RouteParamsIdentifier(nameof(UserGuid))]
[Identifier(nameof(UserGuid), nameof(EventGuid))]
public readonly partial record struct EventIdentifier
{
    public required Guid EventGuid { get; init; }
    public required Guid UserGuid { get; init; }

    public static EventIdentifier New(UserContext userContext) =>
        Create(userContext.Guid, Guid.NewGuid());
}

[RouteParamsIdentifier]
[Identifier(nameof(CategoryName))]
public readonly partial record struct LocationCategoryIdentifier
{
    public required string CategoryName { get; init; }
}

[RouteParamsIdentifier]
[Identifier(nameof(LocationHash))]
public readonly partial record struct LocationIdentifier
{
    public required string LocationHash { get; init; }
}
