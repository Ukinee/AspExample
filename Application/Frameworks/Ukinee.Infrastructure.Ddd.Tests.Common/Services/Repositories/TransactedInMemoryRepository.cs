using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;

public class TransactedInMemoryRepository<TIdentifier, TEntity, TTag>(
    IUnitOfWorkProvider unitOfWorkProvider,
    InMemoryStore<TIdentifier, TEntity> store
) : IEditableTrackedRepository<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    protected InMemoryChangeTracker<TIdentifier, TEntity>? ChangeTracker;

    public async Task AddRange(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        EnsureUnitOfWork();

        if (ChangeTracker == null)
        {
            store.AddRange(entities);

            return;
        }

        foreach (var entity in entities)
        {
            if (store.Exists(entity.Identifier))
                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);

            var entry = ChangeTracker.Entry(entity);

            if (entry.EntityState is not (InMemoryEntityState.Detached or InMemoryEntityState.Deleted))
                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);
        }

        ChangeTracker.AddRange(entities);
    }

    public async Task<UpdateResult<TEntity>> UpdateByIdAsync(
        TIdentifier identifier,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, TEntity> update,
        CancellationToken cancellationToken
    )
    {
        EnsureUnitOfWork();

        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
            return store.UpdateById(identifier, compliedFilter, update);

        var entity = GetEntity(compliedFilter, identifier);

        var updated = update(entity);

        ChangeTracker.UpdateRange([updated]);

        return new UpdateResult<TEntity>(updated, entity);
    }

    public async Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
        IReadOnlyCollection<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        Func<TIdentifier, TEntity, TEntity> update,
        CancellationToken cancellationToken
    )
    {
        EnsureUnitOfWork();

        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
            return store.UpdateManyById(identifiers, compliedFilter, update);

        var result = new List<UpdateResult<TEntity>>();

        foreach (var identifier in identifiers)
        {
            var entity = GetEntity(compliedFilter, identifier);
            var updated = update(entity.Identifier, entity);

            result.Add(new UpdateResult<TEntity>(updated, entity));
        }

        ChangeTracker.UpdateRange(result.Select(e => e.Updated));

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> RemoveRange(
        IReadOnlyCollection<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken
    )
    {
        EnsureUnitOfWork();

        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
            return store.RemoveRange(identifiers, compliedFilter);

        var result = identifiers.Select(identifier => GetEntity(compliedFilter, identifier)).ToList();

        ChangeTracker.RemoveRange(result.Select(e => e.Identifier));

        return result;
    }

#region Read
    public async Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
            return store.FindById(identifier, compliedFilter);

        var entry = ChangeTracker.Entry(identifier);

        if (entry.EntityState is InMemoryEntityState.Deleted)
            return null;

        if (entry.EntityState is not InMemoryEntityState.Detached && entry.Entity != null && compliedFilter.Invoke(entry.Entity))
            return entry.Entity;

        return store.FindById(identifier, compliedFilter);
    }

    public async Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
            return store.Find(specification, compliedFilter);

        return await FindManyAsync(specification, filter, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }

    public async IAsyncEnumerable<TEntity> FindManyAsync(
        Specification<TEntity> specification,
        Expression<Func<TEntity, bool>> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
        {
            await foreach (var entity in store.FindMany(specification, compliedFilter).WithCancellation(cancellationToken))
                yield return entity;

            yield break;
        }

        var trackedEntities = ChangeTracker
            .GetExisting()
            .Select(entry => entry.Entity!)
            .Where(compliedFilter)
            .Where(specification.IsSatisfiedBy)
            .ToAsyncEnumerable();

        var storedEntities = store.FindMany(specification, compliedFilter);
        var comparer = EqualityComparer<TEntity>.Create((e1, e2) => e1 != null && e2 != null && e1.Identifier.Equals(e2.Identifier), e => e.Identifier.GetHashCode());

        await foreach (var entity in trackedEntities.Union(storedEntities, comparer).WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    public async IAsyncEnumerable<TEntity> FindManyByIdAsync(
        IEnumerable<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var compliedFilter = filter.Compile();

        if (ChangeTracker == null)
        {
            await foreach (var entity in store.FindManyById(identifiers, compliedFilter).WithCancellation(cancellationToken))
            {
                yield return entity;
            }

            yield break;
        }

        foreach (var identifier in identifiers)
        {
            var entry = ChangeTracker.Entry(identifier);

            switch (entry.EntityState)
            {
                case InMemoryEntityState.Deleted: break;

                case InMemoryEntityState.Added:
                case InMemoryEntityState.Modified:
                    if (entry.Entity != null && compliedFilter.Invoke(entry.Entity))
                        yield return entry.Entity;

                    break;

                case InMemoryEntityState.Detached:
                default:
                    var existing = store.FindById(identifier, compliedFilter);

                    if (existing != null)
                        yield return existing;

                    break;
            }
        }
    }
#endregion

#region ChangeTracker
    private TEntity GetEntity(Func<TEntity, bool> filter, TIdentifier identifier)
    {
        var entry = ChangeTracker!.Entry(identifier);

        if (entry.EntityState is InMemoryEntityState.Deleted)
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

        var entity = entry.Entity ?? store.FindById(identifier, filter);

        if (entity == null || !filter(entity))
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

        return entity;
    }

    protected virtual void EnsureUnitOfWork()
    {
        var uow = unitOfWorkProvider.Current;

        if (uow == null)
        {
            Debug.Assert(ChangeTracker == null, "_changeTracker != null", $"Must be null because {nameof(unitOfWorkProvider)} does not contain transaction.");

            return;
        }

        uow.ThrowIfNotAffiliatedWith<TTag>();

        var ownerType = store.GetType();

        if (uow.HasAsPart(ownerType))
            return;

        ChangeTracker = new InMemoryChangeTracker<TIdentifier, TEntity>();

        var part = new InMemoryUnitOfWorkPart<TIdentifier, TEntity>(ownerType, store, ChangeTracker, () => ChangeTracker = null);

        uow.RegisterPart(part);
    }
#endregion
}
