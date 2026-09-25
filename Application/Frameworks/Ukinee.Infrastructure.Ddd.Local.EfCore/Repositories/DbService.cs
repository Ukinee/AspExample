using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories
{
    public class DbService<TIdentifier, TEntity, TTag>(IDbContextFactory<TaggedDbContext<TTag>> contextFactory)
        : ISynchronizationDataSource<TEntity>, IEditableTrackedRepository<TIdentifier, TEntity>
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

        public async Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Set<TEntity>().AsNoTracking().WhereIdEquals(identifier).SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async IAsyncEnumerable<TEntity> FindManyByIdAsync(
            IEnumerable<TIdentifier> identifiers,
            Expression<Func<TEntity, bool>> filter,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var enumerable = context.Set<TEntity>().AsNoTracking().WhereIdIn(identifiers).AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async IAsyncEnumerable<TEntity> FindManyAsync(
            Specification<TEntity> specification,
            Expression<Func<TEntity, bool>> filter,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var enumerable = context.Set<TEntity>().AsNoTracking().Where(filter).WithSpecification(specification).AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Set<TEntity>().AsNoTracking().Where(filter).WithSpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddRange(IReadOnlyCollection<TEntity> entities)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Set<TEntity>().AddRange(entities);

            await context.SaveChangesAsync();
        }

        public async Task<UpdateResult<TEntity>> UpdateByIdAsync(
            TIdentifier identifier,
            UpdateLock mode,
            Expression<Func<TEntity, bool>> filter,
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

            return new UpdateResult<TEntity>(entity, newEntity);
        }

        public async Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
            IReadOnlyCollection<TIdentifier> identifiers,
            UpdateLock mode,
            Expression<Func<TEntity, bool>> filter,
            Func<TIdentifier, TEntity, TEntity> update,
            CancellationToken cancellationToken
        )
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var entities = await context
                .Set<TEntity>()
                .WhereIdIn(identifiers)
                .Where(filter)
                .ToListAsync(cancellationToken);
            
            var result = new List<UpdateResult<TEntity>>(identifiers.Count);
            
            result.AddRange(entities.Select(entity => new UpdateResult<TEntity>(entity, update(entity.Identifier, entity))));

            await context.SaveChangesAsync(cancellationToken);

            return result;
        }

        public async Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifiers, Expression<Func<TEntity, bool>> filter)
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
