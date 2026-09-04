using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Common.Exceptions;

public abstract class EntityNotFoundExceptionBase : Exception
{
    protected EntityNotFoundExceptionBase(string message) : base(message) { }
}

public class EntityNotFoundException<TIdentifier, TEntity> : EntityNotFoundExceptionBase
where TEntity : IEntity<TIdentifier>
{
    public EntityNotFoundException()
        : base($"Entity of type {typeof(TEntity).Name} search criteria not found") { }

    public EntityNotFoundException(TIdentifier entityId)
        : base($"Entity of type {typeof(TEntity).Name} with ID {entityId} not found") { }

    public EntityNotFoundException(IEnumerable<TIdentifier> entityIds)
        : base($"Entity of type {typeof(TEntity).Name} with IDs {string.Join(", ", entityIds)} not found") { }
}
