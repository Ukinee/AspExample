using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class GetOrCreateEntityUseCase<TIdentifier, TPayload, TEntity>(IEntityEnsureExistsCreator<TIdentifier, TPayload, TEntity> entityEnsureExistsCreator) : IGetOrCreateEntityUseCase<TIdentifier, TPayload, TEntity>
where TIdentifier : struct
where TEntity : IEntity<TIdentifier>
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken)
    {
        return await entityEnsureExistsCreator.GetOrCreateAsync(userContext, identifier, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads, CancellationToken cancellationToken)
    {
        return await entityEnsureExistsCreator.GetOrCreateAsync(userContext, payloads, cancellationToken);
    }
}
