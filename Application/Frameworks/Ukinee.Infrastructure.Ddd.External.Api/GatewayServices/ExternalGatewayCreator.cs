using System.Net.Http.Json;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

public class ExternalGatewayCreator<TTag, TParams, TIdentifier, TPayload, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : IExternalGatewayCreator<TIdentifier, TPayload, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public async Task<IReadOnlyCollection<TResponse>> CreateAsync(UserContext userContext, IReadOnlyCollection<TPayload> payloads, CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.CreateMany<TEntity>();

        var client = CreateClient();

        var response = await client.PostAsJsonAsync(requestUri, payloads, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TResponse>>(jsonOptionsProvider.Options, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    public async Task<IReadOnlyCollection<TResponse>> EnsureExistsAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads, CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.EnsureExistsMany<TEntity>();

        var client = CreateClient();

        var response = await client.PostAsJsonAsync(requestUri, payloads, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TResponse>>(jsonOptionsProvider.Options, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient(typeof(TTag).Name);
    }
}