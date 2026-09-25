using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;

public record UpsertCachedEntitiesCommand<TIdentifier, TEntity>(UserContext UserContext, IReadOnlyCollection<TEntity> Entities) : IRequest
where TEntity : IEntity<TIdentifier>;

public record RemoveCachedEntitiesCommand<TIdentifier, TEntity>(UserContext UserContext, IReadOnlyCollection<TIdentifier> Identifiers) : IRequest
where TEntity : IEntity<TIdentifier>;

public record InvalidateCacheCommand<TIdentifier, TEntity>(UserContext UserContext, IReadOnlyCollection<TIdentifier> Identifiers) : IRequest
where TEntity : IEntity<TIdentifier>;
