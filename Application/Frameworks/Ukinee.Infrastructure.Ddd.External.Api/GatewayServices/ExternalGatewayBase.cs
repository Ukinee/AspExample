using System.Net.Http.Json;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

public abstract class ExternalGatewayBase<TTag, TParams, TIdentifier, TEntity>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
)
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    protected JsonSerializerOptions JsonOptions => jsonOptionsProvider.Options;

    private HttpClient CreateClient() =>
        httpClientFactory.CreateClient(typeof(TTag).Name);

    protected static HttpRequestMessage CreateRequest(
        UserContext? userContext,
        HttpMethod method,
        string requestUri
    )
    {
        var request = new HttpRequestMessage(method, requestUri);

        if (userContext.HasValue)
            request.Options.Set(RequestOptionsKeys.UserContext, userContext.Value);

        return request;
    }

    protected HttpRequestMessage CreateJsonRequest<TBody>(
        UserContext? userContext,
        HttpMethod method,
        string requestUri,
        TBody body
    )
    {
        var request = CreateRequest(userContext, method, requestUri);
        request.Content = JsonContent.Create(body, options: JsonOptions);

        return request;
    }

    protected async Task<TResponse?> ReadJsonAsync<TResponse>(
        HttpRequestMessage request,
        bool allowNotFound,
        CancellationToken cancellationToken
    )
    {
        var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);

        if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    protected async IAsyncEnumerable<TResponse> ReadJsonStreamAsync<TResponse>(
        HttpRequestMessage request,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var client = CreateClient();

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        await foreach (var dto in response.Content.ReadFromJsonAsAsyncEnumerable<TResponse>(JsonOptions, cancellationToken))
        {
            if (dto is not null)
                yield return dto;
        }
    }

    protected async Task SendNoResponseAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
