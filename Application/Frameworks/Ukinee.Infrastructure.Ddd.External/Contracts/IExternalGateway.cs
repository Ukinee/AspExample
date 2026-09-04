using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Contracts;

public interface IExternalGateway<in TIdentifier, TResponse>
where TIdentifier : notnull
{
    public Task<TResponse?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken);
    public IAsyncEnumerable<TResponse> FindManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken);

    public IAsyncEnumerable<TResponse> GetAllAsync(UserContext userContext, CancellationToken cancellationToken);

    public Task DeleteAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}

public interface IExternalGatewayUpdater<TIdentifier, TUpdatePayload, TResponse>
where TIdentifier : notnull
{
    public Task<TResponse> UpdateAsync(UserContext userContext, TIdentifier identifier, TUpdatePayload updatePayload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TResponse>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads, CancellationToken cancellationToken);
}

public interface IExternalGatewayCreator<TIdentifier, TCreatePayload, TResponse>
{
    public Task<IReadOnlyCollection<TResponse>> CreateAsync(UserContext userContext, IReadOnlyCollection<TCreatePayload> payloads, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TResponse>> EnsureExistsAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads, CancellationToken cancellationToken);
}
