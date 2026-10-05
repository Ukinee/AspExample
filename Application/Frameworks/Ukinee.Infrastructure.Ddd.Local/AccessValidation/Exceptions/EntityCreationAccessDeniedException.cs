using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Exceptions;

public abstract class EntityCreationAccessDeniedException : Exception
{
    protected EntityCreationAccessDeniedException(string message) : base(message) { }
}

public class EntityCreationAccessDeniedException<TIdentifier, TEntity> : EntityCreationAccessDeniedException
where TEntity : IEntity<TIdentifier>
{
    public EntityCreationAccessDeniedException(UserContext userContext, TIdentifier identifier)
        : base($"User {userContext.Guid} doesn't have access to created {typeof(TEntity).Name} {identifier}")
    {
        UserContext = userContext;
        Identifiers = [identifier];
    }

    public EntityCreationAccessDeniedException(UserContext userContext, IEnumerable<TIdentifier> identifiers)
        : base($"User {userContext.Guid} doesn't have access to multiple of created {typeof(TEntity).Name} entities")
    {
        UserContext = userContext;
        Identifiers = identifiers.ToList();
    }

    public UserContext UserContext { get; }
    public IReadOnlyCollection<TIdentifier> Identifiers { get; }
}
