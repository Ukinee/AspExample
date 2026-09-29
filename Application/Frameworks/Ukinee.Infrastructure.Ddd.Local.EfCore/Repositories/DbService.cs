using System.Diagnostics;
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
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Local.EfCore.UnitOfWork;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories
{
    public class DbService<TIdentifier, TEntity, TTag>(
        IUnitOfWorkProvider unitOfWorkProvider,
        TaggedDbContext<TTag> context
    ) : ISynchronizationDataSource<TEntity>, IEditableTrackedRepository<TIdentifier, TEntity>, IDisposable, IAsyncDisposable
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    {
        public async IAsyncEnumerable<TEntity> GetAll([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var enumerable = context.Set<TEntity>().AsNoTracking().AsAsyncEnumerable();

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            return await context.Set<TEntity>().AsNoTracking().WhereIdEquals(identifier).SingleOrDefaultAsync(cancellationToken);
        }

        public async IAsyncEnumerable<TEntity> FindManyByIdAsync(
            IEnumerable<TIdentifier> identifiers,
            Expression<Func<TEntity, bool>> filter,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
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
            var enumerable = context.Set<TEntity>().AsNoTracking().Where(filter).WithSpecification(specification).AsAsyncEnumerable().WithCancellation(cancellationToken);

            await foreach (var entity in enumerable)
            {
                yield return entity;
            }
        }

        public async Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            return await context.Set<TEntity>().AsNoTracking().Where(filter).WithSpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddRange(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
        {
            await EnsureUnitOfWork(cancellationToken);

            context.Set<TEntity>().AddRange(entities);
        }

        public async Task<UpdateResult<TEntity>> UpdateByIdAsync(
            TIdentifier identifier,
            Expression<Func<TEntity, bool>> filter,
            Func<TEntity, TEntity> updateFactory,
            CancellationToken cancellationToken
        )
        {
            await EnsureUnitOfWork(cancellationToken);

            var entity = await context.Set<TEntity>().Where(filter).WhereIdEquals(identifier).SingleOrDefaultAsync(cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            var newEntity = updateFactory(entity);

            context.Entry(entity).CurrentValues.SetValues(newEntity);

            return new UpdateResult<TEntity>(entity, newEntity);
        }

        public async Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
            IReadOnlyCollection<TIdentifier> identifiers,
            Expression<Func<TEntity, bool>> filter,
            Func<TIdentifier, TEntity, TEntity> update,
            CancellationToken cancellationToken
        )
        {
            await EnsureUnitOfWork(cancellationToken);

            var entities = await context
                .Set<TEntity>()
                .Where(filter)
                .WhereIdIn(identifiers)
                .ToListAsync(cancellationToken);

            var result = new List<UpdateResult<TEntity>>(identifiers.Count);

            foreach (var entity in entities)
            {
                var updatedEntity = update(entity.Identifier, entity);

                context.Entry(entity).CurrentValues.SetValues(updatedEntity);

                var updateResult = new UpdateResult<TEntity>(updatedEntity, entity);
                result.Add(updateResult);
            }

            return result;
        }

        public async Task<IReadOnlyCollection<TEntity>> RemoveRange(
            IReadOnlyCollection<TIdentifier> identifiers,
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken
        )
        {
            await EnsureUnitOfWork(cancellationToken);

            var deletedEntities = await context.Set<TEntity>().AsNoTracking().Where(filter).WhereIdIn(identifiers).ToListAsync(); // todo to execute delete? 

            if (deletedEntities.Count == 0)
                return [];

            context.Set<TEntity>().RemoveRange(deletedEntities);

            return deletedEntities;
        }

        private async Task EnsureUnitOfWork(CancellationToken cancellationToken)
        {
            var uow = unitOfWorkProvider.Current;

            if (uow == null)
            {
                Debug.Assert(
                    context.Database.CurrentTransaction == null,
                    "context.Database.CurrentTransaction == null",
                    $"Must be null because {nameof(unitOfWorkProvider)} does not contain transaction."
                );

                return;
            }

            uow.ThrowIfNotAffiliatedWith<TTag>();

            var ownerType = context.GetType();

            if (uow.HasAsPart(ownerType))
                return;

            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var part = new DatabaseUnitOfWorkPart<TTag>(transaction, context, ownerType);

            uow.RegisterPart(part);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
        }
    }
}
