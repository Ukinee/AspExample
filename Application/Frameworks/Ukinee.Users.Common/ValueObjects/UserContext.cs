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
    public required Guid Guid { get; init; }
    public required bool IsAuthenticated { get; init; }

    public required ImmutableArray<string> Roles { get; init; }

    public required string? Name { get; init; }

    public bool Equals(UserContext other)
    {
        if (Guid != other.Guid)
            return false;

        if (IsAuthenticated != other.IsAuthenticated)
            return false;

        if (Name != other.Name)
            return false;

        if (Roles.Length != other.Roles.Length)
            return false;

        var a = Roles.ToHashSet(StringComparer.Ordinal);
        var b = other.Roles.ToHashSet(StringComparer.Ordinal);

        return a.SetEquals(b);
    }

    public override int GetHashCode()
    {
        var rolesHash = Roles
            .OrderBy(x => x, StringComparer.Ordinal)
            .Aggregate(17, HashCode.Combine);

        return HashCode.Combine(Guid, IsAuthenticated, Name, rolesHash);
    }

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
