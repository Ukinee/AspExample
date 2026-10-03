using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public class TrackedDddBuilder<TTag, TIdentifier, TEntity> : ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    private DddBuilder<TTag, TIdentifier, TEntity> _builder;
    private readonly ServiceLifetime _serviceLifetime;
    private readonly bool _throwOnTransaction;

    public TrackedDddBuilder(DddBuilder<TTag, TIdentifier, TEntity> builder, ServiceLifetime serviceLifetime, bool throwOnTransaction)
    {
        _builder = builder;
        _serviceLifetime = serviceLifetime;
        _throwOnTransaction = throwOnTransaction;
    }

    internal DddFeature<TIdentifier, TEntity> Feature => _builder.Feature;

    TrackedDddBuilder<TTag, TIdentifier, TEntity> ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>.SetAccessPolicy(
        AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition
    )
    {
        var serviceDescriptors = AuthorizationHelperDi.GetDescriptors(policyDefinition);
        _builder.Feature.AccessValidator.AddRange(serviceDescriptors);

        return this;
    }

    TrackedDddBuilder<TTag, TIdentifier, TEntity> ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>.
        SetAccessExpressionProvider<TImplementation>(Func<IServiceProvider, TImplementation> factory)
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ServiceDescriptor.Singleton<TImplementation, TImplementation>(factory.Invoke),
            ServiceDescriptor.Singleton<IEntityReadAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
        ];

        _builder.Feature.AccessValidator.AddRange(descriptors);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAccessExpressionProvider<TImplementation>()
    where TImplementation : class, IEntityAccessExpressionProvider<TIdentifier, TEntity>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ServiceDescriptor.Singleton<TImplementation, TImplementation>(),
            ServiceDescriptor.Singleton<IEntityReadAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
            ServiceDescriptor.Singleton<IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>>(sp => sp.GetRequiredService<TImplementation>()),
        ];

        _builder.Feature.AccessValidator.AddRange(descriptors);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithRepository<TRepository>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    where TRepository : class, IEditableTrackedRepository<TIdentifier, TEntity>
    {
        _builder.SetRepository<TRepository>(lifetime);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAsyncCreateUseCase<TCreatePayload, TValidator, TFactory>()
    where TValidator : class, IValidator<IEnumerable<TCreatePayload>>
    where TFactory : class, IEntityAsyncCreateFactory<TCreatePayload, TEntity>
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<IEntityAsyncCreateFactory<TCreatePayload, TEntity>, TFactory>(_serviceLifetime),
            ..PayloadHelper.Service<IValidator<IEnumerable<TCreatePayload>>, TValidator>(_serviceLifetime),
        ];

        _builder
            .SetEntityCreateFeature<AsyncFactoryTrackedEntityCreator<TIdentifier, TEntity, TCreatePayload>, TCreatePayload, CreateEntityUseCase<TEntity, TCreatePayload>>(
                _serviceLifetime,
                extensions,
                _throwOnTransaction
            );

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetCreateUseCase<TCreatePayload, TValidator, TFactory>()
    where TValidator : class, IValidator<IEnumerable<TCreatePayload>>
    where TFactory : class, ICreateEntityFactory<TCreatePayload, TEntity>
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<ICreateEntityFactory<TCreatePayload, TEntity>, TFactory>(_serviceLifetime),
            ..PayloadHelper.Service<IValidator<IEnumerable<TCreatePayload>>, TValidator>(_serviceLifetime),
        ];

        _builder.SetEntityCreateFeature<FactoryTrackedEntityCreator<TIdentifier, TEntity, TCreatePayload>, TCreatePayload, CreateEntityUseCase<TEntity, TCreatePayload>>(
            _serviceLifetime,
            extensions,
            _throwOnTransaction
        );

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> AddUpdateUseCase<TUpdatePayload, TValidator, TFactory>()
    where TValidator : class, IValidator<IEnumerable<TUpdatePayload>>
    where TFactory : class, IUpdateEntityFactory<TUpdatePayload, TEntity>
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<IUpdateEntityFactory<TUpdatePayload, TEntity>, TFactory>(_serviceLifetime),
            ..PayloadHelper.Service<IValidator<IEnumerable<TUpdatePayload>>, TValidator>(_serviceLifetime),
        ];

        _builder
            .AddPayloadEntityUpdateFeature<TrackedPayloadEntityUpdater<TIdentifier, TEntity, TUpdatePayload>, TUpdatePayload,
                UpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>>(_serviceLifetime, extensions, _throwOnTransaction);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> AddRequestHandlerNoValidation<THandler, TPayload, TResponse>()
    where THandler : class, IRequestHandler<PayloadRequest<TPayload, TResponse>, IReadOnlyCollection<TResponse>>
    {
        _builder.AddRequestHandlerFeature<THandler, TPayload, TResponse>(_serviceLifetime, []);

        return this;
    }

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithGetOrCreate<TCreatePayload>()
    {
        _builder.AddGetOrCreateFeature<
            TrackedEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>,
            TCreatePayload,
            GetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>
        >(_serviceLifetime, [], _throwOnTransaction);

        return this;
    }
}

public interface ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAccessPolicy(AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition);

    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAccessExpressionProvider<TImplementation>(Func<IServiceProvider, TImplementation> factory)
    where TImplementation : class, IEntityAccessExpressionProvider<TIdentifier, TEntity>;
    
    public TrackedDddBuilder<TTag, TIdentifier, TEntity> SetAccessExpressionProvider<TImplementation>()
    where TImplementation : class, IEntityAccessExpressionProvider<TIdentifier, TEntity>;
}
