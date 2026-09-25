using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

public class ExternalGatewayUpdater<TTag, TParams, TIdentifier, TPayload, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : ExternalGatewayBase<TTag, TParams, TIdentifier, TEntity>(jsonOptionsProvider, httpClientFactory), IExternalGatewayUpdater<TIdentifier, TPayload, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public async Task<TResponse> UpdateAsync(
        UserContext userContext,
        TIdentifier identifier,
        TPayload updatePayload,
        CancellationToken cancellationToken
    )
    {
        var idParams = TParams.FromIdentifier(identifier);
        var requestUri = RelationalPathUtils.Update<TEntity, TPayload>(idParams.Path);
        using var request = CreateJsonRequest(userContext, HttpMethod.Put, requestUri, updatePayload);

        return await ReadJsonAsync<TResponse>(request, false, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    public async Task<IReadOnlyCollection<TResponse>> UpdateAsync(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.UpdateMany<TEntity, TPayload>();
        using var request = CreateJsonRequest(userContext, HttpMethod.Put, requestUri, payloads);

        return await ReadJsonAsync<List<TResponse>>(request, false, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }
}
