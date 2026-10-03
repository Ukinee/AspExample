using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Users;

public static class ExampleUserFactory
{
    public static readonly UserContext User1 = new UserContext {
        Guid = new Guid("8b2a0971-b14b-4195-9076-1eed55bfa2ff"),
        IsAuthenticated = true,
        Roles = [],
        Name = "Kappa",
    };

    public static readonly UserContext User2 = new UserContext {
        Guid = new Guid("924f965c-2e03-43e1-a5a0-cb53d9b1d14f"),
        IsAuthenticated = true,
        Roles = [TestingAuthorizationPolicyDefinition.Vip],
        Name = "KappaPride",
    };

    public static readonly UserContext User3 = new UserContext {
        Guid = new Guid("386820d2-2be0-4e58-b505-adae4be83f3e"),
        IsAuthenticated = true,
        Roles = [TestingAuthorizationPolicyDefinition.Vip],
        Name = "321",
    };

    public static readonly UserContext UserGuest = UserContext.Guest;

    public static readonly UserContext UserAdmin1 = new UserContext {
        Guid = new Guid("41fed65c-2810-4f7f-9a95-369484ba86df"),
        IsAuthenticated = true,
        Roles = [UserRoles.Admin],
        Name = "TestsAdmin",
    };

    public static readonly UserContext UserAdmin2 = new UserContext {
        Guid = new Guid("41fed65c-2810-4f7f-9a95-369484ba86df"),
        IsAuthenticated = true,
        Roles = [UserRoles.Admin],
        Name = "TestsAdmin",
    };
}
