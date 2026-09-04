using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

//todo: add UserContext support

public class ExternalGateway<TTag, TParams, TIdentifier, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : IExternalGateway<TIdentifier, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
where TResponse : class
{
    public async Task<TResponse?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        var idParams = TParams.FromIdentifier(identifier);
        var requestUri = RelationalPathUtils.Find<TEntity>(idParams.Path);

        var client = CreateClient();

        var response = await client.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(jsonOptionsProvider.Options, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> FindManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.FindMany<TEntity>();

        var resuest = new SearchByIdRequest<TIdentifier> {
            Identifiers = identifiers.ToList(),
        };

        var client = CreateClient();

        var response = await client.PostAsJsonAsync(requestUri, resuest, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseByteStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var responseDtoStream = JsonSerializer.DeserializeAsyncEnumerable<TResponse>(responseByteStream, jsonOptionsProvider.Options, cancellationToken);

        await foreach (var responseDto in responseDtoStream)
        {
            if (responseDto == null)
                continue;

            yield return responseDto;
        }
    }

    public async IAsyncEnumerable<TResponse> GetAllAsync(UserContext userContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.FindAll<TEntity>();

        var client = CreateClient();

        var responseDtoStream = client.GetFromJsonAsAsyncEnumerable<TResponse>(requestUri, jsonOptionsProvider.Options, cancellationToken);

        await foreach (var responseDto in responseDtoStream)
        {
            if (responseDto == null)
                continue;

            yield return responseDto;
        }
    }

    public async Task DeleteAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var requestUri = RelationalPathUtils.DeleteMany<TEntity>();

        var client = CreateClient();

        var body = JsonContent.Create(identifiers, options: jsonOptionsProvider.Options);

        var message = new HttpRequestMessage(HttpMethod.Delete, requestUri) {
            Content = body,
        };

        var response = await client.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient(typeof(TTag).Name);
    }
}
