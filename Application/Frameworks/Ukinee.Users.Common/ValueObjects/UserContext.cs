using System.Collections.Immutable;

namespace Ukinee.Users.Common.ValueObjects;

public static class UserPolicies
{
    public const string AdminPolicy = "Admin";
    public const string LoggedInPolicy = "LoggedInPolicy";
}

public static class UserRoles
{
    public const string Admin = "Admin";
}

public readonly record struct UserContext
{
    public const string GuidHeader = "X-User-Guid";
    public const string RolesHeader = "X-User-Roles";
    public const string NameHeader = "X-User-Name";

    public required Guid Guid { get; init; }
    public required bool IsAuthenticated { get; init; }

    public bool IsAdmin => Roles.Any(role => role == UserRoles.Admin);

    public required ImmutableArray<string> Roles { get; init; }

    public required string? Name { get; init; }

    public static UserContext Elevated => new UserContext {
        Guid = Guid.Parse("8bf896e5-82c3-4150-87a5-1867183a8888"),
        Roles = [UserRoles.Admin],
        Name = "Elevated User",
        IsAuthenticated = true,
    };

    public static UserContext Test => new UserContext {
        Guid = Guid.Parse("5c0f1cdb-7694-4edb-b45c-b7ce78aeb400"),
        Roles = [],
        Name = "Test User",
        IsAuthenticated = true,
    };

    public static UserContext Guest => new UserContext {
        Guid = Guid.Parse("ba0ca027-d1bb-497d-8114-7f4357ac999c"),
        Roles = [],
        Name = "Guest User",
        IsAuthenticated = false,
    };

    public string VisibleName => Name == null
        ? $"{{User ({Guid})}}"
        : $"{{{Name} ({Guid})}}";
}
