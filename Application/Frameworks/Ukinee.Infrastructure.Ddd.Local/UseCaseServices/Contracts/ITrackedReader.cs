using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

public interface ITrackedReader<TIdentifier, TEntity> : IIdentifierReader<TIdentifier, TEntity>, IEntityReader<TIdentifier, TEntity>, ISpecificationReader<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>;
