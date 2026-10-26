using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories.Transacted;

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
    private List<SnapshotEntry>? _snapshot;

    private readonly record struct SnapshotEntry(
        TIdentifier Identifier,
        TEntity? Original,
        long OriginalVersion
    );

    public Type OwnerType => ownerType;
    public bool CanRollbackPartialCommit => true;

    public ValueTask PrepareToCommitAsync(CancellationToken cancellationToken)
    {
        changeTracker.Freeze();

        foreach (var entry in changeTracker.GetAll())
        {
            switch (entry.EntityState)
            {
                case InMemoryEntityState.Modified:
                case InMemoryEntityState.Deleted:
                    if (!store.Exists(entry.Identifier))
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

    public async ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return;

        var all = changeTracker.GetAll().ToList();

        await store.WriteAsync(
            session =>
            {
                var snapshot = new List<SnapshotEntry>(all.Count);

                foreach (var entry in all)
                {
                    var original = session.FindById(entry.Identifier);
                    var version = session.GetExistingVersionOrZero(entry.Identifier);
                    snapshot.Add(new SnapshotEntry(entry.Identifier, original, version));
                }

                _snapshot = snapshot;

                foreach (var entry in all)
                {
                    switch (entry.EntityState)
                    {
                        case InMemoryEntityState.Modified:
                        case InMemoryEntityState.Deleted: {
                            var currentVersion = session.GetVersion(entry.Identifier);

                            if (currentVersion != entry.OriginalVersion)
                                throw new ConcurrencyException(entry.Identifier, entry.OriginalVersion, currentVersion);

                            break;
                        }

                        case InMemoryEntityState.Added:
                            if (session.Exists(entry.Identifier))
                                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entry.Identifier);

                            break;

                        case InMemoryEntityState.Detached: break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (var entry in all)
                {
                    switch (entry.EntityState)
                    {
                        case InMemoryEntityState.Added: session.UpsertNew(entry.Entity!); break;
                        case InMemoryEntityState.Modified: session.UpsertIfVersion(entry.Entity!, entry.OriginalVersion); break;
                        case InMemoryEntityState.Deleted: session.RemoveIfVersion(entry.Identifier, entry.OriginalVersion); break;
                    }
                }
            },
            cancellationToken
        );

        _isCompleted = true;
        onComplete.Invoke();
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return ValueTask.CompletedTask;

        _isCompleted = true;
        _snapshot = null;
        onComplete.Invoke();

        return ValueTask.CompletedTask;
    }

    public async ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken cancellationToken)
    {
        if (!_isCompleted || _snapshot is null)
            return false;

        var snapshot = _snapshot;
        _snapshot = null;

        await store.WriteAsync(
            session =>
            {
                foreach (var entry in snapshot)
                {
                    if (entry.Original is null)
                        session.ForceRemove(entry.Identifier);
                    else
                        session.ForceUpsert(entry.Original, entry.OriginalVersion);
                }
            },
            cancellationToken
        );

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isCompleted)
            return;

        await RollbackAsync(CancellationToken.None);
    }
}
