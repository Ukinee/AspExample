using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;

public class ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>(
    IMapService<TResponse, TEntity> mapService,
    IExternalGatewayCreator<TIdentifier, TCreatePayload, TResponse> externalGatewayCreator
) : IEntityCreator<TCreatePayload, TEntity>, IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var response = await CreateAsync(userContext, [payload], cancellationToken);

        return response.Single();
    }

    public async Task<IReadOnlyCollection<TEntity>> CreateAsync(UserContext userContext, IReadOnlyCollection<TCreatePayload> payloads, CancellationToken cancellationToken)
    {
        var response = await externalGatewayCreator.CreateAsync(userContext, payloads, cancellationToken);

        return await mapService.Map(response);
    }

    public async Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var getOrCreateRequest = new GetOrCreateRequest<TIdentifier, TCreatePayload> {
            Identifier = identifier,
            Payload = payload,
        };

        var response = await GetOrCreateAsync(userContext, [getOrCreateRequest], cancellationToken);

        return response.Single();
    }

    public async Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads, CancellationToken cancellationToken)
    {
        var response = await externalGatewayCreator.EnsureExistsAsync(userContext, payloads, cancellationToken);

        return await mapService.Map(response);
    }
}
