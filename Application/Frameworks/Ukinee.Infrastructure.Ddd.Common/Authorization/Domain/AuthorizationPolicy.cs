using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public enum AuthorizedOperation
{
    Create,
    Read,
    Update,
    Delete,
}

public enum AccessLevel
{
    Guest,
    LoggedIn,
    Owner,
    Admin,
}

public record AuthorizationPolicy<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required AccessLevel CreateLevel { get; init; }
    public required AccessLevel ReadLevel { get; init; }
    public required AccessLevel UpdateLevel { get; init; }
    public required AccessLevel DeleteLevel { get; init; }

    public required Expression<Func<TIdentifier, Guid>>? OwnerUserIdentifierInEntityIdentifierAccessor { get; init; }

    public AccessLevel OperationFor(AuthorizedOperation operation)
    {
        return operation switch {
            AuthorizedOperation.Create => CreateLevel,
            AuthorizedOperation.Read => ReadLevel,
            AuthorizedOperation.Update => UpdateLevel,
            AuthorizedOperation.Delete => DeleteLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}

public static class AuthorizationPolicy
{
    public static AuthorizationPolicy<TIdentifier, TEntity> LoggedInReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Admin,
            ReadLevel = AccessLevel.LoggedIn,
            UpdateLevel = AccessLevel.Admin,
            DeleteLevel = AccessLevel.Admin,
            OwnerUserIdentifierInEntityIdentifierAccessor = null!,
        };

    public static AuthorizationPolicy<TIdentifier, TEntity> GuestReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Admin,
            ReadLevel = AccessLevel.Guest,
            UpdateLevel = AccessLevel.Admin,
            DeleteLevel = AccessLevel.Admin,
            OwnerUserIdentifierInEntityIdentifierAccessor = null!,
        };

    public static AuthorizationPolicy<TIdentifier, TEntity> OwnerOnly<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Owner,
            ReadLevel = AccessLevel.Owner,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicy<TIdentifier, TEntity> GuestReadAndOwnerEdit<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Owner,
            ReadLevel = AccessLevel.Guest,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicy<TIdentifier, TEntity> LoggedInReadAndOwnerEdit<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = AccessLevel.Owner,
            ReadLevel = AccessLevel.LoggedIn,
            UpdateLevel = AccessLevel.Owner,
            DeleteLevel = AccessLevel.Owner,
            OwnerUserIdentifierInEntityIdentifierAccessor = identifierUserGuid,
        };
}
