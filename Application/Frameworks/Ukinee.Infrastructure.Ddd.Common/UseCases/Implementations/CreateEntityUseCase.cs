using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class CreateEntityUseCase<TPayload, TEntity>(IEntityCreator<TPayload, TEntity> entityCreator) : ICreateEntityUseCase<TPayload, TEntity>
where TEntity : class, IEntity
{
    public virtual async Task<TEntity> Execute(UserContext userContext, TPayload payload) =>
        await entityCreator.CreateAsync(userContext, payload, CancellationToken.None);

    public async Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<TPayload> payloads) =>
        await entityCreator.CreateAsync(userContext, payloads, CancellationToken.None);
}
