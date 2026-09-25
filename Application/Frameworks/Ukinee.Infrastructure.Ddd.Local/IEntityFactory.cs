using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local;

public interface IEntityCreateFactory<in TCreatePayload, out TEntity>
where TEntity : class
{
    public TEntity Create(UserContext userContext, TCreatePayload payload);
}

public interface IEntityAsyncCreateFactory<in TCreatePayload, TEntity>
where TEntity : class
{
    public ValueTask<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken);
}

public interface IEntityUpdateFactory<in TUpdatePayload, TEntity>
where TEntity : class
{
    public TEntity Update(UserContext userContext, TEntity old, TUpdatePayload payload);
}

public interface IEntityAsyncUpdateFactory<in TUpdatePayload, TEntity>
where TEntity : class
{
    public ValueTask<TEntity> Update(UserContext userContext, TEntity old, TUpdatePayload payload);
}
