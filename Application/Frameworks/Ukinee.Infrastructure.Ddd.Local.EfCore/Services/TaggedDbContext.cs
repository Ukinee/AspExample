using Microsoft.EntityFrameworkCore;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

public abstract class TaggedDbContext<TTag>(DbContextOptions options) : DbContext(options);

public abstract class TaggedDbContext<TContext, TTag>(DbContextOptions<TContext> options) : TaggedDbContext<TTag>(options)
where TContext : TaggedDbContext<TContext, TTag>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var hasVersionedEntities = modelBuilder
            .Model.GetEntityTypes()
            .Any(e => typeof(IEntityWithVersion).IsAssignableFrom(e.ClrType));

        if (!hasVersionedEntities)
            return;

        modelBuilder.Entity<ChangeSequenceEntry>(b =>
            {
                b.ToTable(ChangeSequenceUtils<TTag>.TableName);
                b.HasKey(x => x.Tag);
                b.Property(x => x.Value).IsConcurrencyToken();

                b.HasData(
                    new ChangeSequenceEntry {
                        Tag = ChangeSequenceUtils<TTag>.TagName,
                        Value = 0,
                    }
                );
            }
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityWithVersion).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IEntityWithVersion.RowVersion))
                    .IsConcurrencyToken();
            }
        }
    }
}
