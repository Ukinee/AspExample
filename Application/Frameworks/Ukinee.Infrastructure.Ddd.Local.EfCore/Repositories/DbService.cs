using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories
{
    public class DbService<TIdentifier, TEntity, TTag>(IDbContextFactory<TaggedDbContext<TTag>> contextFactory) : ISynchronizationDataSource<TEntity>, IEditableTrackedRepository<TIdentifier, TEntity>
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    {
        public async IAsyncEnumerable<TEntity> GetAll([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var enumerable = context.Set<TEntity>().AsNoTracking().AsAsyncEnumerable();

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async Task<TEntity?> FindByIdAsync(TIdentifier identifier, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Set<TEntity>().AsNoTracking().WhereIdEquals(identifier).SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async IAsyncEnumerable<TEntity> FindManyByIdAsync(IEnumerable<TIdentifier> identifiers, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var enumerable = context.Set<TEntity>().AsNoTracking().WhereIdIn(identifiers).AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var enumerable = context.Set<TEntity>().WithSpecification(specification).AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Set<TEntity>().WithSpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddRange(IReadOnlyCollection<TEntity> entities)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Set<TEntity>().AddRange(entities);

            await context.SaveChangesAsync();
        }

        public async Task<TEntity> UpdateByIdAsync(
            TIdentifier identifier,
            UpdateLock mode,
            Func<TEntity, TEntity> updateFactory,
            CancellationToken cancellationToken
        )
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var entity = await context.Set<TEntity>().WhereIdEquals(identifier).SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            var newEntity = updateFactory(entity);

            context.Entry(entity).CurrentValues.SetValues(newEntity);

            await context.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifiers)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var deletedEntities = await context.Set<TEntity>().AsNoTracking().WhereIdIn(identifiers).ToListAsync(); // todo to execute delete? 

            if (deletedEntities.Count == 0)
                return [];

            context.Set<TEntity>().RemoveRange(deletedEntities);
            await context.SaveChangesAsync();

            return deletedEntities;
        }
    }
}
