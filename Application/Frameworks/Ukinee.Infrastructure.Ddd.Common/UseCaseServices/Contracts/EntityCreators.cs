using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IEntityCreator<in TCreatePayload, TEntity>
where TEntity : IEntity
{
    public Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> CreateAsync(UserContext userContext, IReadOnlyCollection<TCreatePayload> payloads, CancellationToken cancellationToken);
}

public interface IEntityEnsureExistsCreator<TIdentifier, TCreatePayload, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TCreatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> payloads, CancellationToken cancellationToken);
}