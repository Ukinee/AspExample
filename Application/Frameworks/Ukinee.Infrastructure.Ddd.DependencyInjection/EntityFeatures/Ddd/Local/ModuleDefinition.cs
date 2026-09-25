using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

// ReSharper disable once CheckNamespace
namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core
{
    public partial record ModuleDefinition<TTag>
    {
        public record InMemoryEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;
        public record HonestDeletionEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;

        public record SoftDeletionEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>
        where TEntity : IEntityWithSoftDelete<TEntity>;
    }
}
