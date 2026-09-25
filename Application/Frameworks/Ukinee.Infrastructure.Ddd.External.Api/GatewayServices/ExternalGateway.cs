using System.Runtime.CompilerServices;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;

public class ExternalGateway<TTag, TParams, TIdentifier, TEntity, TResponse>(
    IJsonOptionsProvider<ExternalGatewayTag> jsonOptionsProvider,
    IHttpClientFactory httpClientFactory
) : ExternalGatewayBase<TTag, TParams, TIdentifier, TEntity>(jsonOptionsProvider, httpClientFactory), IExternalGateway<TIdentifier, TResponse>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
where TResponse : class
{
    public async Task<TResponse?> FindByIdAsync(
        UserContext? userContext,
        TIdentifier identifier,
        CancellationToken cancellationToken
    )
    {
        var idParams = TParams.FromIdentifier(identifier);
        var requestUri = RelationalPathUtils.Find<TEntity>(idParams.Path);
        using var request = CreateRequest(userContext, HttpMethod.Get, requestUri);

        return await ReadJsonAsync<TResponse>(request, true, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> FindManyByIdAsync(
        UserContext? userContext,
        IEnumerable<TIdentifier> identifiers,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.FindMany<TEntity>();
        var payload = new SearchByIdRequest<TIdentifier> { Identifiers = identifiers.ToList() };
        using var request = CreateJsonRequest(userContext, HttpMethod.Post, requestUri, payload);

        await foreach (var dto in ReadJsonStreamAsync<TResponse>(request, cancellationToken))
            yield return dto;
    }

    public async IAsyncEnumerable<TResponse> GetAllAsync(
        UserContext? userContext,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.FindAll<TEntity>();
        using var request = CreateRequest(userContext, HttpMethod.Get, requestUri);

        await foreach (var dto in ReadJsonStreamAsync<TResponse>(request, cancellationToken))
            yield return dto;
    }

    public async Task DeleteAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        var requestUri = RelationalPathUtils.DeleteMany<TEntity>();
        using var request = CreateJsonRequest(userContext, HttpMethod.Delete, requestUri, identifiers);

        await SendNoResponseAsync(request, cancellationToken);
    }
}
