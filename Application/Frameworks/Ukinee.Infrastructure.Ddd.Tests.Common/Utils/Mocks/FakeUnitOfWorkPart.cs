using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;

public class RepoA
{
    public const string Prepare = $"{nameof(RepoA)}.{FakeUnitOfWorkPart.Prepare}";
    public const string Commit = $"{nameof(RepoA)}.{FakeUnitOfWorkPart.Commit}";
    public const string Rollback = $"{nameof(RepoA)}.{FakeUnitOfWorkPart.Rollback}";
    public const string TryRollbackPartial = $"{nameof(RepoA)}.{FakeUnitOfWorkPart.TryRollbackPartial}";
    public const string Dispose = $"{nameof(RepoA)}.{FakeUnitOfWorkPart.Dispose}";
}

public class RepoB
{
    public const string Prepare = $"{nameof(RepoB)}.{FakeUnitOfWorkPart.Prepare}";
    public const string Commit = $"{nameof(RepoB)}.{FakeUnitOfWorkPart.Commit}";
    public const string Rollback = $"{nameof(RepoB)}.{FakeUnitOfWorkPart.Rollback}";
    public const string TryRollbackPartial = $"{nameof(RepoB)}.{FakeUnitOfWorkPart.TryRollbackPartial}";
    public const string Dispose = $"{nameof(RepoB)}.{FakeUnitOfWorkPart.Dispose}";
}

public sealed class FakeUnitOfWorkPart : IUnitOfWorkPart
{
    private readonly List<string> _log;
    private readonly bool _failOnCommit;
    private readonly bool _failOnPrepare;
    private readonly bool _failOnRollback;
    private readonly bool _failOnDispose;

    public const string Prepare = "Prepare";
    public const string Commit = "Commit";
    public const string Rollback = "Rollback";
    public const string TryRollbackPartial = "TryRollbackPartial";
    public const string Dispose = "Dispose";

    public FakeUnitOfWorkPart(
        Type ownerType,
        bool isImportant,
        List<string> log,
        bool failOnCommit = false,
        bool failOnPrepare = false,
        bool failOnRollback = false,
        bool failOnDispose = false
    )
    {
        OwnerType = ownerType;
        IsImportant = isImportant;
        _log = log;
        _failOnCommit = failOnCommit;
        _failOnPrepare = failOnPrepare;
        _failOnRollback = failOnRollback;
        _failOnDispose = failOnDispose;
    }

    public Type OwnerType { get; }
    public bool IsImportant { get; }

    public bool Committed { get; private set; }
    public bool PartialRollbackCalled { get; private set; }
    public bool RollbackCalled { get; private set; }
    public bool DisposeCalled { get; private set; }

    public ValueTask PrepareToCommitAsync(CancellationToken ct)
    {
        _log.Add($"{OwnerType.Name}.{Prepare}");

        if (_failOnPrepare)
            throw new InvalidOperationException("prepare failed");

        return ValueTask.CompletedTask;
    }

    public ValueTask CommitAsync(CancellationToken ct)
    {
        if (DisposeCalled || RollbackCalled || Committed)
            return ValueTask.CompletedTask;

        _log.Add($"{OwnerType.Name}.{Commit}");

        if (_failOnCommit)
            throw new InvalidOperationException("commit failed");

        Committed = true;

        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken ct)
    {
        _log.Add($"{OwnerType.Name}.{Rollback}");

        if (DisposeCalled || RollbackCalled || Committed)
            return ValueTask.CompletedTask;

        if (_failOnRollback)
            throw new InvalidOperationException("rollback failed");

        RollbackCalled = true;

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken ct)
    {
        _log.Add($"{OwnerType.Name}.{TryRollbackPartial}");

        if (!Committed)
            return ValueTask.FromResult(false);

        PartialRollbackCalled = true;

        return ValueTask.FromResult(true);
    }

    public ValueTask DisposeAsync()
    {
        _log.Add($"{OwnerType.Name}.{Dispose}");

        if (DisposeCalled || RollbackCalled || Committed)
            return ValueTask.CompletedTask;

        if (_failOnDispose)
            throw new InvalidOperationException("dispose failed");

        DisposeCalled = true;

        return ValueTask.CompletedTask;
    }
}
