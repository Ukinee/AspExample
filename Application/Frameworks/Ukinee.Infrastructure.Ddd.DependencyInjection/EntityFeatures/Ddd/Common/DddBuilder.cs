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

    public DddBuilder<TIdentifier, TEntity> SetRepository<TImplementation>()
    where TImplementation : class, IEditableTrackedRepository<TIdentifier, TEntity>
    {
        Feature.Repository.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IEditableTrackedRepository<TIdentifier, TEntity>, TImplementation>(),
            ..PayloadHelper.Singleton<ITrackedRepository<TIdentifier, TEntity>, TImplementation>(),
        ];

        Feature.Repository.AddRange(descriptors);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetEntityCreateFeature<TImplementation, TPayload, TUseCase>(
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IEntityCreator<TPayload, TEntity>
    where TUseCase : class, ICreateEntityUseCase<TPayload, TEntity>
    {
        Feature.EntityCreator.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IEntityCreator<TPayload, TEntity>, TImplementation>(),
            ..PayloadHelper.Singleton<ICreateEntityUseCase<TPayload, TEntity>, TUseCase>(),
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
        IEnumerable<ServiceDescriptor> extensions,
        bool throwOnTransaction
    )
    where TImplementation : class, IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
    where TUseCase : class, IUpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>, TImplementation>(),
            ..PayloadHelper.Singleton<IUpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>, TUseCase>(),
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

    internal DddBuilder<TIdentifier, TEntity> AddRequestHandlerFeature<THandler, TPayload, TResponse>(IEnumerable<ServiceDescriptor> extensions)
    where THandler : class, IRequestHandler<PayloadRequest<TPayload, TResponse>, IReadOnlyCollection<TResponse>>
    {
        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IRequestHandler<PayloadRequest<TPayload, TResponse>, IReadOnlyCollection<TResponse>>, THandler>(), ..extensions,
        ];

        Feature.CustomRequestHandlers.AddRange(descriptors);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetDeltaEntityUpdater<TImplementation, TUseCase>(bool throwOnTransaction)
    where TImplementation : class, IDeltaEntityUpdater<TEntity>
    where TUseCase : class, IDeltaUpdateEntityUseCase<TEntity>
    {
        Feature.DeltaEntityUpdater.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IDeltaUpdateEntityUseCase<TEntity>, TUseCase>(), ..PayloadHelper.Singleton<IDeltaEntityUpdater<TEntity>, TImplementation>(),
        ];

        Feature.DeltaEntityUpdater.AddRange(descriptors);
        
        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IDeltaEntityUpdater<TEntity>, TransactionThrowingDeltaEntityUpdater<TEntity>>(Feature.DeltaEntityUpdater);
        }

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetEntityRemover<TImplementation, TUseCase>(bool throwOnTransaction)
    where TImplementation : class, IEntityRemover<TIdentifier, TEntity>
    where TUseCase : class, IRemoveEntityUseCase<TEntity>
    {
        Feature.EntityRemover.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IRemoveEntityUseCase<TEntity>, TUseCase>(), ..PayloadHelper.Singleton<IEntityRemover<TIdentifier, TEntity>, TImplementation>(),
        ];

        Feature.EntityRemover.AddRange(descriptors);
        
        if (throwOnTransaction)
        {
            ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, TransactionThrowingEntityRemover<TIdentifier, TEntity>>(Feature.EntityRemover);
        }

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetIdentifierReader<TImplementation, TUseCase>()
    where TImplementation : class, IIdentifierReader<TIdentifier, TEntity>
    where TUseCase : class, IGetEntityUseCase<TIdentifier, TEntity>
    {
        Feature.IdentifierReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IGetEntityUseCase<TIdentifier, TEntity>, TUseCase>(),
            ..PayloadHelper.Singleton<IIdentifierReader<TIdentifier, TEntity>, TImplementation>(),
        ];

        Feature.IdentifierReader.AddRange(descriptors);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetEntityReader<TImplementation, TUseCase>()
    where TImplementation : class, IEntityReader<TIdentifier, TEntity>
    where TUseCase : class, IGetAllEntityUseCase<TIdentifier, TEntity>
    {
        Feature.EntityReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IGetAllEntityUseCase<TIdentifier, TEntity>, TUseCase>(),
            ..PayloadHelper.Singleton<IEntityReader<TIdentifier, TEntity>, TImplementation>(),
        ];

        Feature.EntityReader.AddRange(descriptors);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> SetSpecificationReader<TImplementation>()
    where TImplementation : class, ISpecificationReader<TIdentifier, TEntity>
    {
        Feature.SpecificationReader.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Singleton<ISpecificationReader<TIdentifier, TEntity>, TImplementation>(),];

        Feature.SpecificationReader.AddRange(descriptors);

        return this;
    }

    public DddBuilder<TIdentifier, TEntity> AddGetOrCreateFeature<TImplementation, TCreatePayload, TUseCase>(IEnumerable<ServiceDescriptor> extensions)
    where TImplementation : class, IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
    where TUseCase : class, IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>
    {
        Feature.GetOrCreate.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Singleton<IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>, TImplementation>(),
            ..PayloadHelper.Singleton<IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>, TUseCase>(),
            ..extensions
        ];

        Feature.GetOrCreate.AddRange(descriptors);

        return this;
    }
}
