using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IEntityCreator<TEntity, in TCreatePayload>
where TEntity : IEntity
{
    public Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> CreateAsync(UserContext userContext, IReadOnlyCollection<TCreatePayload> payloads, CancellationToken cancellationToken);
}

public interface IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>
where TEntity : IEntity<TIdentifier>
{
    public Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TCreatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads, CancellationToken cancellationToken);
}