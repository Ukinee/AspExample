using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;

public class DddBuilder<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    internal readonly DddFeature<TIdentifier, TEntity> Feature;

    public DddBuilder(DddFeature<TIdentifier, TEntity> feature)
    {
        Feature = feature;
    }

    public DddBuilder<TIdentifier, TEntity> WithRepository<TImplementation>()
    where TImplementation : class, IEditableTrackedRepository<TIdentifier, TEntity>
    {
        Feature.Repository = new DddFeature<TIdentifier, TEntity>.RepositoryDefinition() {
            Type = typeof(TImplementation),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetEntityCreateFeature<TImplementation, TPayload, TUseCase>(Type[] extensions)
    where TImplementation : class, IEntityCreator<TPayload, TEntity>
    where TUseCase : class, ICreateEntityUseCase<TPayload, TEntity>
    {
        Feature.EntityCreator = new DddFeature<TIdentifier, TEntity>.EntityCreatorDefinition() {
            Types = [typeof(TImplementation), ..extensions],
            UseCase = typeof(TUseCase),
        };

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> AddPayloadEntityUpdateFeature<TImplementation, TUpdatePayload, TUseCase>(Type[] extensions)
    where TImplementation : class, IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
    {
        var definition = new DddFeature<TIdentifier, TEntity>.PayloadEntityUpdaterDefinition {
            Types = [typeof(TImplementation), ..extensions],
            UseCase = typeof(TUseCase),
        };

        Feature.PayloadEntityUpdaters.Add(definition);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> WithDeltaEntityUpdater<TImplementation, TUseCase>()
    where TImplementation : class, IDeltaEntityUpdater<TEntity>
    where TUseCase : class, IUpdateEntityUseCase<TEntity>
    {
        Feature.DeltaEntityUpdater = new DddFeature<TIdentifier, TEntity>.DeltaEntityUpdaterDefinition() {
            Type = typeof(TImplementation),
            UseCase = typeof(TUseCase),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> WithEntityRemover<TImplementation, TUseCase>()
    where TImplementation : class, IEntityRemover<TIdentifier, TEntity>
    where TUseCase : class, IRemoveEntityUseCase<TEntity>
    {
        Feature.EntityRemover = new DddFeature<TIdentifier, TEntity>.EntityRemoverDefinition() {
            Type = typeof(TImplementation),
            UseCase = typeof(TUseCase),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> WithIdentifierReader<TImplementation, TUseCase>()
    where TImplementation : class, IIdentifierReader<TIdentifier, TEntity>
    where TUseCase : class, IGetEntityUseCase<TIdentifier, TEntity>
    {
        Feature.IdentifierReader = new DddFeature<TIdentifier, TEntity>.IdentifierReaderDefinition() {
            Type = typeof(TImplementation),
            UseCase = typeof(TUseCase),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> WithEntityReader<TImplementation>()
    where TImplementation : class, IEntityReader<TIdentifier, TEntity>
    {
        Feature.EntityReader = new DddFeature<TIdentifier, TEntity>.EntityReaderDefinition {
            Type = typeof(TImplementation),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> WithSpecificationReader<TImplementation>()
    where TImplementation : class, ISpecificationReader<TIdentifier, TEntity>
    {
        Feature.SpecificationReader = new DddFeature<TIdentifier, TEntity>.SpecificationReaderDefinition() {
            Type = typeof(TImplementation),
        };

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> AddGetOrCreateFeature<TImplementation, TCreatePayload, TUseCase>(Type[] extensions)
    where TImplementation : class, IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
    where TUseCase : class, IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>
    {
        Feature.GetOrCreate = new DddFeature<TIdentifier, TEntity>.GetOrCreateFeatureDefinition {
            Types = [..extensions, typeof(TUseCase)],
            Creator = typeof(TImplementation),
        };

        return this;
    }
}
