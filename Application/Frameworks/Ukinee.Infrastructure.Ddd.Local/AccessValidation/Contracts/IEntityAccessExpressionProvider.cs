using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;

public interface IEntityAccessExpressionProvider<TIdentifier, TEntity> :
    IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>,
    IEntityReadAccessExpressionProvider<TIdentifier, TEntity>,
    IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>,
    IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>;

public interface IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public abstract Task<Expression<Func<TEntity, bool>>> GetCreateExpression(UserContext userContext);
}

public interface IEntityReadAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public abstract Task<Expression<Func<TEntity, bool>>> GetReadExpression(UserContext userContext);
}

public interface IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public abstract Task<Expression<Func<TEntity, bool>>> GetUpdateExpression(UserContext userContext);
}

public interface IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public abstract Task<Expression<Func<TEntity, bool>>> GetDeleteExpression(UserContext userContext);
}
