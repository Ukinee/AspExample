using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;

public class ExternalGatewayPayloadUpdaterAdapter<TIdentifier, TUpdatePayload, TEntity, TResponse>(
    IMapService<TResponse, TEntity> mapService,
    IExternalGatewayUpdater<TIdentifier, TUpdatePayload, TResponse> externalGatewayUpdater
) : IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier id, TUpdatePayload payload, CancellationToken cancellationToken)
    {
        var response = await externalGatewayUpdater.UpdateAsync(userContext, id, payload, cancellationToken);

        return await mapService.Map(response);
    }

    public async Task<IReadOnlyCollection<TEntity>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads, CancellationToken cancellationToken)
    {
        var response = await externalGatewayUpdater.UpdateAsync(userContext, payloads, cancellationToken);

        return await mapService.Map(response);
    }
}
