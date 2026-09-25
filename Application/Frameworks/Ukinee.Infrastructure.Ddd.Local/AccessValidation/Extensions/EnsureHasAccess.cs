using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Extensions;

public static class EnsureHasAccessExtensions
{
    extension<TIdentifier, TEntity>(IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public void EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
    
    extension<TIdentifier, TEntity>(IEntityReadAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public void EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
    
    extension<TIdentifier, TEntity>(IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public void EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
    
    extension<TIdentifier, TEntity>(IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public void EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
