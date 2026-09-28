using System.Diagnostics.CodeAnalysis;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Common.Exceptions;

public abstract class EntityNotFoundExceptionBase : Exception
{
    protected EntityNotFoundExceptionBase(string message) : base(message) { }
}

public static class EntityNotFoundException
{
    public static void ThrowIfAnyMissing<TIdentifier, TEntity>(IReadOnlyCollection<TIdentifier> sourceIds, IReadOnlyCollection<TIdentifier> resultIds)
    where TEntity : IEntity<TIdentifier>
    {
        if (sourceIds.Count != resultIds.Count)
        {
            Throw<TIdentifier, TEntity>(sourceIds, resultIds);
        }
    }

    public static void ThrowIfAnyMissing<TIdentifier, TEntity>(IReadOnlyCollection<TIdentifier> sourceIds, IReadOnlyCollection<TEntity> resultEntities)
    where TEntity : IEntity<TIdentifier>
    {
        if (sourceIds.Count != resultEntities.Count)
        {
            Throw<TIdentifier, TEntity>(sourceIds, resultEntities.Select(x => x.Identifier));
        }
    }

    public static void ThrowIfAnyMissing<TIdentifier, TEntity>(IReadOnlyCollection<TEntity> sourceEntities, IReadOnlyCollection<TEntity> resultEntities)
    where TEntity : IEntity<TIdentifier>
    {
        if (sourceEntities.Count != resultEntities.Count)
        {
            Throw<TIdentifier, TEntity>(sourceEntities.Select(x => x.Identifier), resultEntities.Select(x => x.Identifier));
        }
    }

    [DoesNotReturn]
    private static void Throw<TIdentifier, TEntity>(IEnumerable<TIdentifier> sourceIds, IEnumerable<TIdentifier> resultIds)
    where TEntity : IEntity<TIdentifier>
    {
        var missing = sourceIds.Except(resultIds);

        throw new EntityNotFoundException<TIdentifier, TEntity>(missing);
    }
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
