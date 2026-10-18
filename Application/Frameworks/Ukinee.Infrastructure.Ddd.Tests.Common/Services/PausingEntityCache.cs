using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Services;

public sealed class PausingEntityCache<TIdentifier, TEntity> : IEntityCache<TIdentifier, TEntity>
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct
{
    private enum PauseState { Idle, Armed, Paused, Released }

    private readonly IEntityCache<TIdentifier, TEntity> _inner;
    private readonly object _gate = new();

    private PauseState _state = PauseState.Idle;
    private TIdentifier? _armedId;
    private TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ManualResetEventSlim _released = new(false);

    public PausingEntityCache(IEntityCache<TIdentifier, TEntity> inner) => _inner = inner;

    public void ArmBeforeUpsert(TIdentifier id)
    {
        lock (_gate)
        {
            if (_state != PauseState.Idle)
                throw new InvalidOperationException($"Cannot arm: state is {_state}.");

            _armedId = id;
            _reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _released = new ManualResetEventSlim(false);
            _state = PauseState.Armed;
        }
    }

    public Task WaitForUpsertAttemptAsync()
    {
        lock (_gate)
        {
            if (_state is not (PauseState.Armed or PauseState.Paused))
                throw new InvalidOperationException($"Nothing to wait for: state is {_state}.");

            return _reached.Task;
        }
    }

    public void ReleaseUpsert()
    {
        ManualResetEventSlim released;
        lock (_gate)
        {
            if (_state != PauseState.Paused)
                throw new InvalidOperationException($"Nothing paused to release: state is {_state}.");

            _state = PauseState.Released;
            released = _released;
        }

        released.Set();
    }

    public CacheLookupResult<TEntity> Lookup(TIdentifier id) => _inner.Lookup(id);

    public void Upsert(TEntity entity)
    {
        ManualResetEventSlim? gate = null;
        TaskCompletionSource? reached = null;

        lock (_gate)
        {
            if (_state == PauseState.Armed
                && _armedId is { } armed
                && EqualityComparer<TIdentifier>.Default.Equals(armed, entity.Identifier))
            {
                gate = _released;
                reached = _reached;
                _state = PauseState.Paused;
            }
        }

        if (gate is not null)
        {
            reached!.TrySetResult();
            gate.Wait();
        }

        _inner.Upsert(entity);
    }

    public void Upsert(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
            Upsert(entity);
    }

    public void MarkNotExists(TIdentifier id) => _inner.MarkNotExists(id);
    public void MarkNotExists(IEnumerable<TIdentifier> ids) => _inner.MarkNotExists(ids);
    public void Invalidate(TIdentifier id) => _inner.Invalidate(id);
    public void Invalidate(IEnumerable<TIdentifier> ids) => _inner.Invalidate(ids);
    public void CleanupExpired() => _inner.CleanupExpired();
    public void Clear() => _inner.Clear();
}