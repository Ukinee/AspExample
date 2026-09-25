// ReSharper disable once CheckNamespace
namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core
{
    public partial record ModuleDefinition<TTag>
    {
        public record RemoteEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;
    }
}
