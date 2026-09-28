using MediatR;

namespace Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Contacts;

public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create<TTag>();
}

public interface IUnitOfWorkProvider
{
    public IEditableUnitOfWork? Current { get; }
}

public interface IUnitOfWork : IAsyncDisposable
{
    public Type Tag { get; }

    public Task CommitAsync(CancellationToken cancellationToken);
    public Task RollbackAsync(CancellationToken cancellationToken);
}

public interface IEditableUnitOfWork : IUnitOfWork
{
    public void ThrowIfNotAffiliatedWith<TTag>();
    public bool HasAsPart(Type ownerType);

    public void RegisterNotification(INotification notification);
    public void RegisterPart(IUnitOfWorkPart part);
}

public interface IUnitOfWorkPart : IAsyncDisposable
{
    public Type OwnerType { get; }

    public bool IsImportant { get; }

    public ValueTask PrepareToCommitAsync(CancellationToken cancellationToken);
    public ValueTask CommitAsync(CancellationToken cancellationToken);
    
    public ValueTask RollbackAsync(CancellationToken cancellationToken);
    public ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken cancellationToken);
    
}
