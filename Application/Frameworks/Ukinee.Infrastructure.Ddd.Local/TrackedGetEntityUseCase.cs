using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local;

public class TrackedGetEntityUseCase<TIdentifier, TEntity>(ITrackedReader<TIdentifier, TEntity> reader) : GetEntityUseCase<TIdentifier, TEntity>(reader, reader)
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    protected readonly ITrackedReader<TIdentifier, TEntity> Reader = reader;
}
