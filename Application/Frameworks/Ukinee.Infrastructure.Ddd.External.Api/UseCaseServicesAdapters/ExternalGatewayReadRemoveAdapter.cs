using System.Runtime.CompilerServices;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Exceptions;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;

public class ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>(
    IMapService<TResponse, TEntity> mapService,
    IExternalGateway<TIdentifier, TResponse> externalGateway
) : IIdentifierReader<TIdentifier, TEntity>, IEntityReader<TIdentifier, TEntity>, IEntityRemover<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var responseStream = externalGateway.GetAllAsync(userContext, cancellationToken);

        await foreach (var response in responseStream) // todo: chunks
        {
            yield return await mapService.Map(response);
        }
    }

    public async Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        var response = await externalGateway.FindByIdAsync(userContext, identifier, cancellationToken);

        if (response is null)
            return null;

        return await mapService.Map(response);
    }

    public async IAsyncEnumerable<TEntity> FindManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var responseStream = externalGateway.FindManyByIdAsync(userContext, identifiers, cancellationToken);

        await foreach (var response in responseStream) // todo: chunks
        {
            yield return await mapService.Map(response);
        }
    }

    public async Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var identifiersCollection = identifiers as ICollection<TIdentifier> ?? identifiers.ToList();

        var result = await FindManyByIdAsync(userContext, identifiersCollection, cancellationToken)
            .ToDictionaryAsync(x => x.Identifier, cancellationToken: cancellationToken);

        if (result.Count != identifiersCollection.Count)
        {
            var missing = identifiersCollection.Except(result.Keys);

            throw new EntityNotFoundOrNotExistsException<TIdentifier, TEntity>(missing, [], userContext);
        }

        return result;
    }

    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await externalGateway.DeleteAsync(userContext, entities.Select(e => e.Identifier).ToArray(), cancellationToken);
    }

    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        await externalGateway.DeleteAsync(userContext, identifiers, cancellationToken);
    }
}