using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class GetEntityUseCase<TIdentifier, TEntity>(
    IEntityReader<TIdentifier, TEntity> entityReader,
    IIdentifierReader<TIdentifier, TEntity> identifierReader
) : IGetEntityUseCase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    protected readonly IIdentifierReader<TIdentifier, TEntity> IdentifierReader = identifierReader;

    public IAsyncEnumerable<TEntity> ExecuteAll(UserContext userContext, CancellationToken cancellationToken) =>
        entityReader.GetAllAsync(userContext, cancellationToken);

    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken) =>
        await IdentifierReader.GetByIdAsync(userContext, identifier, cancellationToken);

    public async Task<TEntity?> Execute(UserContext userContext, TIdentifier? identifier, CancellationToken cancellationToken)
    {
        if (identifier is null)
            return null;

        return await Execute(userContext, identifier.Value, cancellationToken);
    }

    public IAsyncEnumerable<TEntity> ExecuteSoft(UserContext userContext, IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken) =>
        IdentifierReader.FindManyByIdAsync(userContext, identifiers, cancellationToken);

    public async Task<Dictionary<TIdentifier, TEntity>> ExecuteStrict(
        UserContext userContext,
        IEnumerable<TIdentifier> identifiers,
        CancellationToken ct
    )
    {
        return await IdentifierReader.FindManyByIdStrictAsync(userContext, identifiers, ct);
    }
}
