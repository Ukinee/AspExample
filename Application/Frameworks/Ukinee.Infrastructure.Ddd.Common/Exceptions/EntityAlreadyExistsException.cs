using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Common.Exceptions;

public class EntityAlreadyExistsException<TIdentifier, TEntity> : Exception where TEntity : IEntity<TIdentifier>
{
    public EntityAlreadyExistsException(TIdentifier identifier)
        : base($"Entity of type {typeof(TEntity).Name} with ID {identifier} already exists")
    {
        EntityIdentifier = identifier;
    }

    public TIdentifier EntityIdentifier { get; }
}
