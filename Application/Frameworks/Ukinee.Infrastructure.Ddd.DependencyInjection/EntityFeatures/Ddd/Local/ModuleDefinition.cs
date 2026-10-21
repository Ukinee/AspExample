using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;

// ReSharper disable once CheckNamespace
namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core
{
    public partial record ModuleDefinition<TTag>
    {
        public record InMemoryEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;
        public record HonestDeletionDatabaseEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;

        public record SoftDeletionDatabaseEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>
        where TEntity : IEntityWithSoftDelete<TEntity>;
    }
}
