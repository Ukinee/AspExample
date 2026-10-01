using FluentValidation;
using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Extensions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>(
    IIdentifierReader<TIdentifier, TEntity> reader,
    IEntityCreator<TEntity, TCreatePayload> creator
) : IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var existing = await reader.FindByIdAsync(userContext, identifier, cancellationToken);

        if (existing != null)
            return existing;

        return await creator.CreateAsync(userContext, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(
        UserContext userContext,
        IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads,
        CancellationToken cancellationToken
    )
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

public abstract class TrackedEntityCreatorBase<TIdentifier, TEntity, TCreatePayload>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IValidator<IEnumerable<TCreatePayload>> validationService,
    IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : IEntityCreator<TEntity, TCreatePayload>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var result = await CreateAsync(userContext, [payload], cancellationToken);

        return result.Single();
    }

    public async Task<IReadOnlyCollection<TEntity>> CreateAsync(
        UserContext userContext,
        IReadOnlyCollection<TCreatePayload> payloads,
        CancellationToken cancellationToken
    )
    {
        await validationService.ValidateAndThrowAsync(payloads, cancellationToken);

        List<TEntity> result = new List<TEntity>(payloads.Count);

        foreach (var payload in payloads)
            result.Add(await CreateInternal(userContext, payload, cancellationToken));

        accessProvider.EnsureAccess(userContext, result);

        await repository.AddRange(result, cancellationToken);
        await publisher.PublishCreatedEvent<TIdentifier, TEntity>(userContext, result, cancellationToken);

        return result;
    }

    protected abstract ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
}

public class FactoryTrackedEntityCreator<TIdentifier, TEntity, TCreatePayload>(
    IEntityCreateFactory<TCreatePayload, TEntity> factory,
    IValidator<IEnumerable<TCreatePayload>> validationService,
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : TrackedEntityCreatorBase<TIdentifier, TEntity, TCreatePayload>(repository, validationService, accessProvider, publisher)
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    protected override ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(factory.Create(userContext, payload));
    }
}

public class AsyncFactoryTrackedEntityCreator<TIdentifier, TEntity, TCreatePayload>(
    IEntityAsyncCreateFactory<TCreatePayload, TEntity> factory,
    IValidator<IEnumerable<TCreatePayload>> validationService,
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : TrackedEntityCreatorBase<TIdentifier, TEntity, TCreatePayload>(repository, validationService, accessProvider, publisher)
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    protected override ValueTask<TEntity> CreateInternal(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        return factory.CreateAsync(userContext, payload, cancellationToken);
    }
}
