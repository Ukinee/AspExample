namespace Ukinee.Users.Domain;

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

    public bool IsAdmin => GetRoles().Any(role => role == UserRoles.Admin);

    public required string Roles { get; init; }

    public required string? Name { get; init; }

    public static UserContext Elevated => new UserContext {
        Guid = Guid.Parse("8bf896e5-82c3-4150-87a5-1867183a8888"),
        Roles = UserRoles.Admin,
        Name = "Elevated User",
    };

    public static UserContext Test => new UserContext {
        Guid = Guid.Parse("5c0f1cdb-7694-4edb-b45c-b7ce78aeb400"),
        Roles = "",
        Name = "Test User",
    };

    public static UserContext Invalid => new UserContext {
        Guid = Guid.Empty,
        Roles = null!,
        Name = "Invalid Tracing User",
    };

    public string VisibleName => Name == null
        ? $"{{User ({Guid})}}"
        : $"{{{Name} ({Guid})}}";

    public string[] GetRoles() =>
        Roles.Split(',');
}
