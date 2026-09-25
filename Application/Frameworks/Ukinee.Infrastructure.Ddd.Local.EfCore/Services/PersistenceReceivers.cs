using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

public class DeletableDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>(DbService<TIdentifier, TEntity, TTag> service)
    : DomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>, IEntityWithSoftDelete<TEntity>
where TIdentifier : struct, IEquatable<TIdentifier>
{
    protected override async Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task OnUpdated(IReadOnlyCollection<UpdateResult<TEntity>> updateInfos, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task OnRemoved(IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class HonestDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>(DbService<TIdentifier, TEntity, TTag> service) : DomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
{
    protected override async Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task OnUpdated(IReadOnlyCollection<UpdateResult<TEntity>> updateInfos, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task OnRemoved(IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
