using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local;

public interface ICreateEntityFactory<in TCreatePayload, out TEntity>
where TEntity : class, IEntity
{
    public TEntity Create(UserContext userContext, TCreatePayload payload);
}

public interface IEntityAsyncCreateFactory<in TCreatePayload, TEntity>
where TEntity : class, IEntity
{
    public ValueTask<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
}

public interface IUpdateEntityFactory<in TUpdatePayload, TEntity>
where TEntity : class, IEntity
{
    public TEntity Update(UserContext userContext, TEntity old, TUpdatePayload payload);
}

public interface IEntityAsyncUpdateFactory<in TUpdatePayload, TEntity>
where TEntity : class, IEntity
{
    public ValueTask<TEntity> Update(UserContext userContext, TEntity old, TUpdatePayload payload);
}
