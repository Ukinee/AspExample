using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.UnitOfWork;

public class InMemoryUnitOfWorkPart<TIdentifier, TEntity>(
    Type ownerType,
    InMemoryStore<TIdentifier, TEntity> store,
    InMemoryChangeTracker<TIdentifier, TEntity> changeTracker,
    Action onComplete
) : IUnitOfWorkPart
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    private bool _isCompleted;

    private List<(TIdentifier Identifier, TEntity? Original)>? _snapshot;

    public Type OwnerType => ownerType;

    public bool IsImportant => false;

    public ValueTask PrepareToCommitAsync(CancellationToken cancellationToken)
    {
        changeTracker.Freeze();

        foreach (var entry in changeTracker.GetAll())
        {
            switch (entry.EntityState)
            {
                case InMemoryEntityState.Modified:
                case InMemoryEntityState.Deleted:
                    if (store.Exists(entry.Identifier) == false)
                        throw new EntityNotFoundException<TIdentifier, TEntity>(entry.Identifier);

                    break;

                case InMemoryEntityState.Added:
                    if (store.Exists(entry.Identifier))
                        throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entry.Identifier);

                    break;

                case InMemoryEntityState.Detached: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return ValueTask.CompletedTask;

        var snapshot = new List<(TIdentifier, TEntity?)>();

        var all = changeTracker.GetAll().ToList();
        
        foreach (var entry in all)
        {
            var original = store.FindById(entry.Identifier, _ => true);
            snapshot.Add((entry.Identifier, original));
        }

        _snapshot = snapshot;

        _isCompleted = true;

        var added = all
            .Where(e => e.EntityState == InMemoryEntityState.Added)
            .Select(e => e.Entity!)
            .ToList();

        var modified = all
            .Where(e => e.EntityState == InMemoryEntityState.Modified)
            .Select(e => e.Entity!)
            .ToList();

        var deleted = all
            .Where(e => e.EntityState == InMemoryEntityState.Deleted)
            .Select(e => e.Identifier)
            .ToList();

        store.RemoveRange(deleted, _ => true);

        store.Upsert(added);
        store.Upsert(modified);

        onComplete.Invoke();

        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return ValueTask.CompletedTask;

        _isCompleted = true;
        onComplete.Invoke();

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken cancellationToken)
    {
        if (!_isCompleted || _snapshot is null)
            return ValueTask.FromResult(false);

        var snapshot = _snapshot;
        _snapshot = null;

        var toRemove = new List<TIdentifier>();
        var toRestore = new List<TEntity>();

        foreach (var (identifier, original) in snapshot)
        {
            if (original is null)
                toRemove.Add(identifier);
            else
                toRestore.Add(original);
        }

        if (toRemove.Count > 0)
            store.RemoveRange(toRemove, _ => true);

        if (toRestore.Count > 0)
            store.Upsert(toRestore);

        return ValueTask.FromResult(true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isCompleted)
            return;

        _isCompleted = true;

        await RollbackAsync(CancellationToken.None);
    }
}
