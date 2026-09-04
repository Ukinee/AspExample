using System.Collections;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Exceptions;

public abstract class EntityAccessDeniedExceptionBase : Exception
{
    protected EntityAccessDeniedExceptionBase(string message) : base(message) { }
}

public abstract class EntityNotFoundOrNotExistsExceptionBase : Exception
{
    public abstract IEnumerable NotFoundObjects { get; }
    public abstract IEnumerable AccessDeniedObjects { get; }
}

public class EntityAccessDeniedException<TIdentifier, TEntity> : EntityAccessDeniedExceptionBase
where TEntity : IEntity<TIdentifier>
{
    public EntityAccessDeniedException(UserContext userContext, TIdentifier identifier)
        : base($"User {userContext} doesn't have access to {typeof(TEntity).Name} {identifier}")
    {
        UserContext = userContext;
        Identifiers = [identifier];
    }

    public EntityAccessDeniedException(UserContext userContext, IEnumerable<TIdentifier> identifiers)
        : base($"User {userContext} doesn't have access to multiple {typeof(TEntity).Name} entities")
    {
        UserContext = userContext;
        Identifiers = identifiers.ToList();
    }

    public UserContext UserContext { get; }
    public IReadOnlyCollection<TIdentifier> Identifiers { get; }
}

public class EntityNotFoundOrNotExistsException<TIdentifier, TEntity> : EntityNotFoundOrNotExistsExceptionBase
where TEntity : IEntity<TIdentifier>
{
    public EntityNotFoundOrNotExistsException(IEnumerable<TIdentifier> notFound, IEnumerable<TIdentifier> accessDenied, UserContext userContext)
    {
        NotFound = notFound;
        AccessDenied = accessDenied;
        UserContext = userContext;
    }

    public IEnumerable<TIdentifier> NotFound { get; }
    public IEnumerable<TIdentifier> AccessDenied { get; }
    public UserContext UserContext { get; }

    public override IEnumerable NotFoundObjects => NotFound;
    public override IEnumerable AccessDeniedObjects => AccessDenied;
}
