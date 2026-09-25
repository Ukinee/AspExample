using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface ICreateEntityUseCase<in TPayload, TEntity>
where TEntity : IEntity
{
    public Task<TEntity> Execute(UserContext userContext, TPayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<TPayload> payloads, CancellationToken cancellationToken);
}