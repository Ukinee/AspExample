using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IGetOrCreateEntityUseCase<TIdentifier, TPayload, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : struct
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads, CancellationToken cancellationToken);
}
