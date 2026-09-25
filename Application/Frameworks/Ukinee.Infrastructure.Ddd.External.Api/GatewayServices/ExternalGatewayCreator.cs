using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

public class ExternalGatewayCreator<TTag, TParams, TIdentifier, TPayload, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : ExternalGatewayBase<TTag, TParams, TIdentifier, TEntity>(jsonOptionsProvider, httpClientFactory), IExternalGatewayCreator<TIdentifier, TPayload, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public async Task<IReadOnlyCollection<TResponse>> CreateAsync(
        UserContext userContext,
        IReadOnlyCollection<TPayload> payloads,
        CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.CreateMany<TEntity>();
        using var request = CreateJsonRequest(userContext, HttpMethod.Post, requestUri, payloads);

        return await ReadJsonAsync<List<TResponse>>(request, false, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }

    public async Task<IReadOnlyCollection<TResponse>> EnsureExistsAsync(
        UserContext userContext,
        IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.EnsureExistsMany<TEntity>();
        using var request = CreateJsonRequest(userContext, HttpMethod.Post, requestUri, payloads);

        return await ReadJsonAsync<List<TResponse>>(request, false, cancellationToken) ?? throw new InvalidOperationException("No response returned.");
    }
}
