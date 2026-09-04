using System.Net.Http.Json;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

public class ExternalGatewayUpdater<TTag, TParams, TIdentifier, TPayload, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : IExternalGatewayUpdater<TIdentifier, TPayload, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public async Task<TResponse> UpdateAsync(UserContext userContext, TIdentifier identifier, TPayload updatePayload, CancellationToken cancellationToken)
    {
        var idParams = TParams.FromIdentifier(identifier);

        var requestUri = RelationalPathUtils.Update<TEntity, TPayload>(idParams.Path);

        var client = CreateClient();

        var response = await client.PutAsJsonAsync(requestUri, updatePayload, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(jsonOptionsProvider.Options, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    public async Task<IReadOnlyCollection<TResponse>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads, CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.UpdateMany<TEntity, TPayload>();

        var client = CreateClient();

        var response = await client.PutAsJsonAsync(requestUri, payloads, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TResponse>>(jsonOptionsProvider.Options, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient(typeof(TTag).Name);
    }
}
