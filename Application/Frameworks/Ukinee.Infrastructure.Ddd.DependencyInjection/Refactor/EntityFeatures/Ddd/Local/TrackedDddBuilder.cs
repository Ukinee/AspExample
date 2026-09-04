using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices;
using Ukinee.Infrastructure.Validation.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local;

public class TrackedDddBuilder<TTag, TIdentifier, TEntity> : ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    private DddBuilder<TIdentifier, TEntity> _builder;

    public TrackedDddBuilder(DddBuilder<TIdentifier, TEntity> builder)
    {
        _builder = builder;
    }

    internal DddFeature<TIdentifier, TEntity> Feature => _builder.Feature;

    TrackedDddBuilder<TTag, TIdentifier, TEntity>
        ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>.WithIdentifierAccessValidator<TImplementation>()
    {
        _builder.Feature.Extensions.Add(sc => sc.AddSingleton<IIdentifierEntityAccessValidator<TIdentifier, TEntity>, TImplementation>());

        return this;
    }

    TrackedDddBuilder<TTag, TIdentifier, TEntity>
        ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>.WithIdentifierAccessValidator(Func<IServiceProvider, IIdentifierEntityAccessValidator<TIdentifier, TEntity>> validatorFactory)
    {
        _builder.Feature.Extensions.Add(sc => sc.AddSingleton(validatorFactory.Invoke));

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithRepository<TRepository>()
    where TRepository : class, IEditableTrackedRepository<TIdentifier, TEntity>
    {
        _builder.WithRepository<TRepository>();

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAsyncCreateUseCase<TCreatePayload, TValidator, TFactory>()
    where TValidator : IValidationService<TCreatePayload>
    where TFactory : IEntityAsyncCreateFactory<TCreatePayload, TEntity>
    {
        var factoryType = typeof(TFactory);
        var validatorType = typeof(TValidator);

        Type[] extensions = [factoryType, validatorType];

        _builder.SetEntityCreateFeature<AsyncFactoryTrackedEntityCreator<TIdentifier, TCreatePayload, TEntity>, TCreatePayload, CreateEntityUseCase<TCreatePayload, TEntity>>(extensions);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetCreateUseCase<TCreatePayload, TValidator, TFactory>()
    where TValidator : IValidationService<TCreatePayload>
    where TFactory : IEntityCreateFactory<TCreatePayload, TEntity>
    {
        var factoryType = typeof(TFactory);
        var validatorType = typeof(TValidator);

        Type[] extensions = [factoryType, validatorType];

        _builder.SetEntityCreateFeature<FactoryTrackedEntityCreator<TIdentifier, TCreatePayload, TEntity>, TCreatePayload, CreateEntityUseCase<TCreatePayload, TEntity>>(extensions);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> AddUpdateUseCase<TUpdatePayload, TValidator, TFactory>()
    where TValidator : IValidationService<TUpdatePayload>
    where TFactory : IEntityUpdateFactory<TUpdatePayload, TEntity>
    {
        var factoryType = typeof(TFactory);
        var validatorType = typeof(TValidator);

        Type[] extensions = [factoryType, validatorType];

        _builder.AddPayloadEntityUpdateFeature<TrackedPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>, TUpdatePayload, UpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>>(extensions);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithGet<TUseCase>()
    where TUseCase : GetEntityUseCase<TIdentifier, TEntity>
    {
        _builder.Feature.IdentifierReader = _builder.Feature.IdentifierReader with {
            UseCase = typeof(TUseCase),
        };

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithGetOrCreate<TCreatePayload>()
    {
        _builder.AddGetOrCreateFeature<
            TrackedEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>,
            TCreatePayload,
            GetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>
        >([]);

        return this;
    }
}

public interface ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithIdentifierAccessValidator(Func<IServiceProvider, IIdentifierEntityAccessValidator<TIdentifier, TEntity>> validatorFactory);

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithIdentifierAccessValidator<TImplementation>()
    where TImplementation : class, IIdentifierEntityAccessValidator<TIdentifier, TEntity>;
}