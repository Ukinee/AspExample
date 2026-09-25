using System.Runtime.CompilerServices;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;

public class ExternalSynchronizationDataSourceAdapter<TIdentifier, TEntity, TResponse>(
    IUserContextProvider userContextProvider,
    IExternalGateway<TIdentifier, TResponse> externalGateway,
    IMapService<TResponse, TEntity> mapService
) : ISynchronizationDataSource<TEntity>
where TIdentifier : notnull
where TEntity : IEntity<TIdentifier>
{
    public async IAsyncEnumerable<TEntity> GetAll([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userContext = userContextProvider.GetActiveUserContext();

        var responseStream = externalGateway.GetAllAsync(userContext, cancellationToken);

        await foreach (var response in responseStream)
        {
            yield return await mapService.Map(response);
        }
    }
}
