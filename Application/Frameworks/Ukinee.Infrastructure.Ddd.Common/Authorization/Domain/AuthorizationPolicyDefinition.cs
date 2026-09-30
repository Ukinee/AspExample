using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;



public sealed record AuthorizationPolicyDefinition<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required AccessLevel CreateLevel { get; init; }
    public required AccessLevel ReadLevel { get; init; }
    public required AccessLevel UpdateLevel { get; init; }
    public required AccessLevel DeleteLevel { get; init; }

    public required Expression<Func<TIdentifier, Guid>>? OwnerUserIdentifierInEntityIdentifierAccessor { get; init; }

    public AccessLevel AccessLevelFor(AuthorizedOperation operation)
    {
        switch (operation)
        {
            case AuthorizedOperation.Create: return CreateLevel;
            case AuthorizedOperation.Read: return ReadLevel;
            case AuthorizedOperation.Update: return UpdateLevel;
            case AuthorizedOperation.Delete: return DeleteLevel;
            default: throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }
    }
}



public static class AuthorizationPolicyDefinition
{
    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> LoggedInReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Admin,
            ReadLevel = AccessLevel.LoggedIn,
            UpdateLevel = AccessLevel.Admin,
            DeleteLevel = AccessLevel.Admin,
            OwnerUserIdentifierInEntityIdentifierAccessor = null!,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> GuestReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Admin,
            ReadLevel = AccessLevel.Guest,
            UpdateLevel = AccessLevel.Admin,
            DeleteLevel = AccessLevel.Admin,
            OwnerUserIdentifierInEntityIdentifierAccessor = null!,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> OwnerOnly<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.LoggedIn,
            ReadLevel = AccessLevel.Owner,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> GuestReadAndOwnerEdit<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.LoggedIn,
            ReadLevel = AccessLevel.Guest,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> LoggedInReadAndOwnerEdit<TIdentifier, TEntity>(
        Expression<Func<TIdentifier, Guid>> identifierUserGuid
    )
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.LoggedIn,
            ReadLevel = AccessLevel.LoggedIn,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };
}
