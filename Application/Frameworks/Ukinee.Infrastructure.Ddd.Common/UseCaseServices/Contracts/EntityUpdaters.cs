using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier identifier, TUpdatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads, CancellationToken cancellationToken);
}