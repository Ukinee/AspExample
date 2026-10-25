using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

public class SaveChangesVersionInterceptor<TTag>(IVersionStrategy<TTag> versionStrategy) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        var context = eventData.Context;

        if (context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var versionedEntries = context
            .ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified && e.Entity is IEntityWithVersion)
            .ToList();

        if (versionedEntries.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        long nextVersion = await versionStrategy.GetNextVersionAsync(context, cancellationToken);

        string versionName = nameof(IEntityWithVersion.RowVersion);

        foreach (var entry in versionedEntries)
        {
            entry.Property(versionName).CurrentValue = nextVersion;
            entry.Property(versionName).OriginalValue = nextVersion;
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
