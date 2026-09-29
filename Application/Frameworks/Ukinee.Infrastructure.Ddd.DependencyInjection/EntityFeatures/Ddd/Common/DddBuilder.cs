using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Decorators;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;

public class DddBuilder<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    internal readonly DddFeature<TIdentifier, TEntity> Feature;

    public DddBuilder(DddFeature<TIdentifier, TEntity> feature)
    {
        Feature = feature;
    }

    internal DddBuilder<TIdentifier, TEntity> SetRepository<TImplementation>(ServiceLifetime serviceLifetime)
    where TImplementation : class, IEditableTrackedRepository<TIdentifier, TEntity>
    {
        Feature.Repository.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IEditableTrackedRepository<TIdentifier, TEntity>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<ITrackedRepository<TIdentifier, TEntity>, TImplementation>(serviceLifetime),
        ];

        Feature.Repository.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetEntityCreateFeature<TImplementation, TPayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IEntityCreator<TPayload, TEntity>
    where TUseCase : class, ICreateEntityUseCase<TPayload, TEntity>
    {
        Feature.EntityCreator.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IEntityCreator<TPayload, TEntity>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<ICreateEntityUseCase<TPayload, TEntity>, TUseCase>(serviceLifetime),
            ..extensions,
        ];

        Feature.EntityCreator.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IEntityCreator<TPayload, TEntity>, TransactionThrowingEntityCreator<TPayload, TEntity>>(Feature.EntityCreator);
        }

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> AddPayloadEntityUpdateFeature<TImplementation, TUpdatePayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
    where TUseCase : class, IUpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<IUpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>, TUseCase>(serviceLifetime),
            ..extensions,
        ];

        Feature.PayloadEntityUpdaters.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators
                .Decorate<IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>, TransactionThrowingEntityUpdater<TIdentifier, TEntity, TUpdatePayload>>(
                    Feature.PayloadEntityUpdaters
                );
        }

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> AddRequestHandlerFeature<THandler, TPayload, TResponse>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions
    )
    where THandler : class, IRequestHandler<PayloadRequest<TPayload, TResponse>, IReadOnlyCollection<TResponse>>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IRequestHandler<PayloadRequest<TPayload, TResponse>, IReadOnlyCollection<TResponse>>, THandler>(serviceLifetime), ..extensions,
        ];

        Feature.CustomRequestHandlers.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetDeltaEntityUpdater<TImplementation, TUseCase>(
        ServiceLifetime serviceLifetime,
        bool throwOnTransaction
    )
    where TImplementation : class, IDeltaEntityUpdater<TEntity>
    where TUseCase : class, IDeltaUpdateEntityUseCase<TEntity>
    {
        Feature.DeltaEntityUpdater.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IDeltaUpdateEntityUseCase<TEntity>, TUseCase>(serviceLifetime),
            ..PayloadHelper.Service<IDeltaEntityUpdater<TEntity>, TImplementation>(serviceLifetime),
        ];

        Feature.DeltaEntityUpdater.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IDeltaEntityUpdater<TEntity>, TransactionThrowingDeltaEntityUpdater<TEntity>>(Feature.DeltaEntityUpdater);
        }

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetEntityRemover<TImplementation, TUseCase>(ServiceLifetime serviceLifetime, bool throwOnTransaction)
    where TImplementation : class, IEntityRemover<TIdentifier, TEntity>
    where TUseCase : class, IRemoveEntityUseCase<TEntity>
    {
        Feature.EntityRemover.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IRemoveEntityUseCase<TEntity>, TUseCase>(serviceLifetime),
            ..PayloadHelper.Service<IEntityRemover<TIdentifier, TEntity>, TImplementation>(serviceLifetime),
        ];

        Feature.EntityRemover.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, TransactionThrowingEntityRemover<TIdentifier, TEntity>>(Feature.EntityRemover);
        }

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetIdentifierReader<TImplementation, TUseCase>(ServiceLifetime serviceLifetime)
    where TImplementation : class, IIdentifierReader<TIdentifier, TEntity>
    where TUseCase : class, IGetEntityUseCase<TIdentifier, TEntity>
    {
        Feature.IdentifierReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IGetEntityUseCase<TIdentifier, TEntity>, TUseCase>(serviceLifetime),
            ..PayloadHelper.Service<IIdentifierReader<TIdentifier, TEntity>, TImplementation>(serviceLifetime),
        ];

        Feature.IdentifierReader.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetEntityReader<TImplementation, TUseCase>(ServiceLifetime serviceLifetime)
    where TImplementation : class, IEntityReader<TIdentifier, TEntity>
    where TUseCase : class, IGetAllEntityUseCase<TIdentifier, TEntity>
    {
        Feature.EntityReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IGetAllEntityUseCase<TIdentifier, TEntity>, TUseCase>(serviceLifetime),
            ..PayloadHelper.Service<IEntityReader<TIdentifier, TEntity>, TImplementation>(serviceLifetime),
        ];

        Feature.EntityReader.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> SetSpecificationReader<TImplementation>(ServiceLifetime serviceLifetime)
    where TImplementation : class, ISpecificationReader<TIdentifier, TEntity>
    {
        Feature.SpecificationReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Service<ISpecificationReader<TIdentifier, TEntity>, TImplementation>(serviceLifetime),];

        Feature.SpecificationReader.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TIdentifier, TEntity> AddGetOrCreateFeature<TImplementation, TCreatePayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions
    )
    where TImplementation : class, IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
    where TUseCase : class, IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>
    {
        Feature.GetOrCreate.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>, TUseCase>(serviceLifetime),
            ..extensions
        ];

        Feature.GetOrCreate.AddRange(descriptors);

        return this;
    }
}
