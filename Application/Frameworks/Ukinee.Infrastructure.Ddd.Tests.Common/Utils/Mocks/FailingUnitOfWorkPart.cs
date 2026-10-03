using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;

public sealed class FailingUnitOfWorkPart : IUnitOfWorkPart
{
    public FailingUnitOfWorkPart(Type ownerType, string? message = null)
    {
        OwnerType = ownerType;
        Message = message ?? "Failing part";
    }

    public Type OwnerType { get; }

    public bool IsImportant => true;

    public string Message { get; }

    public bool PartialRollbackCalled { get; private set; }
    public bool RollbackCalled { get; private set; }
    public bool DisposeCalled { get; private set; }

    public ValueTask PrepareToCommitAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask CommitAsync(CancellationToken cancellationToken)
        => throw new InvalidOperationException(Message);

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        RollbackCalled = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken cancellationToken)
    {
        PartialRollbackCalled = true;
        return ValueTask.FromResult(false); // сам коммит не проходил
    }

    public ValueTask DisposeAsync()
    {
        DisposeCalled = true;
        return ValueTask.CompletedTask;
    }
}