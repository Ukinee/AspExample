using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection;

public static class EntityExtensions
{
    //todo: uncomment whenever CS9295 is fixed in this context or write code generator

    // extension<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
    // where TIdentifier : struct, IEquatable<TIdentifier>
    // where TEntity : class, IEntity<TIdentifier>
    // where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
    // {
    //     public TDefinition WithServices(Action<IServiceCollection> action)
    //     {
    //         definition.Services.Add(action);
    //
    //         return definition;
    //     }
    // }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> WithServices(Action<IServiceCollection> action)
        {
            definition.Services.Add(action);

            return definition;
        }
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> WithServices(Action<IServiceCollection> action)
        {
            definition.Services.Add(action);

            return definition;
        }
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>, IEntityWithSoftDelete<TEntity>
    {
        public ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> WithServices(Action<IServiceCollection> action)
        {
            definition.Services.Add(action);

            return definition;
        }
    }

    extension<TTag, TIdentifier, TIdentifierParams, TEntity>(ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> WithServices(Action<IServiceCollection> action)
        {
            definition.Services.Add(action);

            return definition;
        }
    }
}
