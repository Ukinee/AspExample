using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>(
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IIdentifierReader<TIdentifier, TEntity> reader,
    IEntityCreator<TCreatePayload, TEntity> creator
) : IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TCreatePayload payload, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, identifier);

        var existing = await reader.FindByIdAsync(userContext, identifier, cancellationToken);

        if (existing != null)
            return existing;

        return await creator.CreateAsync(userContext, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads, CancellationToken cancellationToken)
    {
        var payloadsDict = payloads.ToDictionary(p => p.Identifier, p => p.Payload);

        var existing = await reader
            .FindManyByIdAsync(userContext, payloadsDict.Keys, cancellationToken)
            .ToListAsync(cancellationToken);

        var missingIdentifiers = payloadsDict.Keys.Except(existing.Select(p => p.Identifier));
        var requests = missingIdentifiers.Select(id => payloadsDict[id]).ToList();

        var created = await creator.CreateAsync(userContext, requests, cancellationToken);

        return existing.Concat(created).ToList();
    }
}

public abstract class TrackedEntityCreatorBase<TIdentifier, TCreatePayload, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : IEntityCreator<TCreatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var result = await CreateAsync(userContext, [payload], cancellationToken);

        return result.Single();
    }

    public async Task<IReadOnlyCollection<TEntity>> CreateAsync(UserContext userContext, IReadOnlyCollection<TCreatePayload> payloads, CancellationToken cancellationToken)
    {
        var result = new List<TEntity>(payloads.Count);

        foreach (var payload in payloads)
            result.Add(await CreateInternal(userContext, payload, cancellationToken));

        identifierEntityAccessValidator.EnsureHasAccess(userContext, result);

        await repository.AddRange(result);
        await publisher.PublishCreatedEvent<TIdentifier, TEntity>(userContext, result);

        return result;
    }

    protected abstract ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
}

public class FactoryTrackedEntityCreator<TIdentifier, TCreatePayload, TEntity>(
    IEntityCreateFactory<TCreatePayload, TEntity> factory,
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : TrackedEntityCreatorBase<TIdentifier, TCreatePayload, TEntity>(repository, identifierEntityAccessValidator, publisher)
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    protected override ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(factory.Create(userContext, payload));
    }
}

public class AsyncFactoryTrackedEntityCreator<TIdentifier, TCreatePayload, TEntity>(
    IEntityAsyncCreateFactory<TCreatePayload, TEntity> factory,
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : TrackedEntityCreatorBase<TIdentifier, TCreatePayload, TEntity>(repository, identifierEntityAccessValidator, publisher)
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    protected override ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        return factory.CreateAsync(userContext, payload, cancellationToken);
    }
}
