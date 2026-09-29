using MediatR;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Domain;

internal sealed class AmbientUnitOfWork : IEditableUnitOfWork
{
    private readonly Func<AmbientUnitOfWork, CancellationToken, Task> _commit;
    private readonly Func<AmbientUnitOfWork, CancellationToken, Task> _rollback;
    private readonly List<INotification> _notifications = [];
    private readonly List<IUnitOfWorkPart> _parts = [];

    private bool _isCompleted;

    private AmbientUnitOfWork(
        Type tag,
        Func<AmbientUnitOfWork, CancellationToken, Task> commit,
        Func<AmbientUnitOfWork, CancellationToken, Task> rollback
    )
    {
        Tag = tag;
        _commit = commit;
        _rollback = rollback;
    }

    public Type Tag { get; }

    public IReadOnlyList<INotification> DeferredNotifications => _notifications;
    public IReadOnlyList<IUnitOfWorkPart> Parts => _parts;

    public static AmbientUnitOfWork Create<TTag>(
        Func<AmbientUnitOfWork, CancellationToken, Task> commit,
        Func<AmbientUnitOfWork, CancellationToken, Task> rollback
    )
    {
        return new AmbientUnitOfWork(
            typeof(TTag),
            commit,
            rollback
        );
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            throw new InvalidOperationException($"{nameof(AmbientUnitOfWork)} is completed and cannot be commited.)");

        _isCompleted = true;

        try
        {
            await _commit.Invoke(this, cancellationToken);
        }
        finally
        {
            Clear();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            throw new InvalidOperationException($"{nameof(AmbientUnitOfWork)} is completed and cannot be rolled back.)");

        _isCompleted = true;

        try
        {
            await _rollback.Invoke(this, cancellationToken);
        }
        finally
        {
            Clear();
        }
    }

    public void ThrowIfNotAffiliatedWith<TTag>()
    {
        if (Tag != typeof(TTag))
            throw new InvalidOperationException($"UnitOfWork is bound to {Tag.Name}, not {typeof(TTag).Name}.");
    }

    public bool HasAsPart(Type ownerType)
    {
        return _parts.Any(x => x.OwnerType == ownerType);
    }

    public void RegisterNotification(INotification notification)
    {
        if (_isCompleted)
            throw new InvalidOperationException($"{nameof(AmbientUnitOfWork)} is completed and cannot accept notifications.)");

        _notifications.Add(notification);
    }

    public void RegisterPart(IUnitOfWorkPart part)
    {
        if (_isCompleted)
            throw new InvalidOperationException($"{nameof(AmbientUnitOfWork)} is completed and cannot accept parts.)");

        if (part.IsImportant && _parts.Any(d => d.IsImportant))
            throw new InvalidOperationException($"{nameof(AmbientUnitOfWork)} cannot accept more than one important part.)");

        _parts.Add(part);
    }

    private void Clear()
    {
        _notifications.Clear();
        _parts.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isCompleted)
            return;

        await RollbackAsync(CancellationToken.None);
    }
}
