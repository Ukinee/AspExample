using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCases;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Decorators;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;

public class DddBuilder<TTag, TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    internal readonly DddFeature<TIdentifier, TEntity> Feature;

    public DddBuilder(DddFeature<TIdentifier, TEntity> feature)
    {
        Feature = feature;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> SetRepository<TImplementation>(ServiceLifetime serviceLifetime)
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

    internal DddBuilder<TTag, TIdentifier, TEntity> SetEntityCreateFeature<TImplementation, TPayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IEntityCreator<TEntity, TPayload>
    where TUseCase : class, ICreateEntityUseCase<TEntity, TPayload>
    {
        Feature.EntityCreator.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IEntityCreator<TEntity, TPayload>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<ICreateEntityUseCase<TEntity, TPayload>, TUseCase>(serviceLifetime),
            ..extensions,
        ];

        Feature.EntityCreator.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IEntityCreator<TEntity, TPayload>, TransactionThrowingEntityCreator<TEntity, TPayload>>(Feature.EntityCreator);
        }
        else
        {
            ServiceDescriptorDecorators
                .Decorate<ICreateEntityUseCase<TEntity, TPayload>, TransactionCreateEntityUseCaseDecorator<TTag, TEntity, TPayload>>(Feature.EntityCreator);
        }

        return this;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> AddPayloadEntityUpdateFeature<TImplementation, TUpdatePayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IPayloadEntityUpdater<TIdentifier, TEntity, TUpdatePayload>
    where TUseCase : class, IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IPayloadEntityUpdater<TIdentifier, TEntity, TUpdatePayload>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>, TUseCase>(serviceLifetime),
            ..extensions,
        ];

        Feature.PayloadEntityUpdaters.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators
                .Decorate<IPayloadEntityUpdater<TIdentifier, TEntity, TUpdatePayload>, TransactionThrowingEntityUpdater<TIdentifier, TEntity, TUpdatePayload>>(
                    Feature.PayloadEntityUpdaters
                );
        }
        else
        {
            ServiceDescriptorDecorators
                .Decorate<IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>,
                    TransactionUpdateEntityUseCaseDecorator<TTag, TIdentifier, TEntity, TUpdatePayload>>(Feature.PayloadEntityUpdaters);
        }

        return this;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> AddRequestHandlerFeature<THandler, TPayload, TResponse>(
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

    internal DddBuilder<TTag, TIdentifier, TEntity> SetDeltaEntityUpdater<TImplementation, TUseCase>(
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
        else
        {
            ServiceDescriptorDecorators.Decorate<IDeltaUpdateEntityUseCase<TEntity>, TransactionDeltaUpdateEntityUseCaseDecorator<TTag, TIdentifier, TEntity>>(
                Feature.DeltaEntityUpdater
            );
        }

        return this;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> SetEntityRemover<TImplementation, TUseCase>(ServiceLifetime serviceLifetime, bool throwOnTransaction)
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
        else
        {
            ServiceDescriptorDecorators.Decorate<IRemoveEntityUseCase<TEntity>, TransactionRemoveEntityUseCaseDecorator<TTag, TIdentifier, TEntity>>(
                Feature.EntityRemover
            );
        }

        return this;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> SetIdentifierReader<TImplementation, TUseCase>(ServiceLifetime serviceLifetime)
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

    internal DddBuilder<TTag, TIdentifier, TEntity> SetEntityReader<TImplementation, TUseCase>(ServiceLifetime serviceLifetime)
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

    internal DddBuilder<TTag, TIdentifier, TEntity> SetSpecificationReader<TImplementation>(ServiceLifetime serviceLifetime)
    where TImplementation : class, ISpecificationReader<TIdentifier, TEntity>
    {
        Feature.SpecificationReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Service<ISpecificationReader<TIdentifier, TEntity>, TImplementation>(serviceLifetime),];

        Feature.SpecificationReader.AddRange(descriptors);

        return this;
    }

    internal DddBuilder<TTag, TIdentifier, TEntity> AddGetOrCreateFeature<TImplementation, TCreatePayload, TUseCase>(
        ServiceLifetime serviceLifetime,
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>
    where TUseCase : class, IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>
    {
        Feature.GetOrCreate.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>, TImplementation>(serviceLifetime),
            ..PayloadHelper.Service<IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>, TUseCase>(serviceLifetime),
            ..extensions
        ];

        Feature.GetOrCreate.AddRange(descriptors);

        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>,
                TransactionThrowingEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>>(Feature.GetOrCreate);
        }
        else
        {
            ServiceDescriptorDecorators.Decorate<IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>,
                TransactionGetOrCreateEntityUseCaseDecorator<TTag, TIdentifier, TEntity, TCreatePayload>>(Feature.GetOrCreate);
        }

        return this;
    }
}
