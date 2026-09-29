using System.Data.Entity;
using Microsoft.EntityFrameworkCore.Storage;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.UnitOfWork;

public class DatabaseUnitOfWorkPart<TTag>(IDbContextTransaction transaction, TaggedDbContext<TTag> context, Type ownerType) : IUnitOfWorkPart
{
    public Type OwnerType => ownerType;
    public bool IsImportant => true;

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }

    public async ValueTask PrepareToCommitAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        await transaction.CommitAsync(cancellationToken);

    }

    public async ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken);
    }

    public ValueTask<bool> TryRollbackPartialCommitAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(false);
    }
}
