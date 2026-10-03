using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Exceptions;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Extensions;

public static class EnsureHasAccessExtensions
{
    extension<TIdentifier, TEntity>(IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public async Task EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            var expression = await accessExpressionProvider.GetCreateExpression(userContext);

            ThrowIfDenied<TIdentifier, TEntity>(expression, userContext, entities);
        }
    }

    extension<TIdentifier, TEntity>(IEntityReadAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public async Task EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            var expression = await accessExpressionProvider.GetReadExpression(userContext);

            ThrowIfDenied<TIdentifier, TEntity>(expression, userContext, entities);
        }
    }

    extension<TIdentifier, TEntity>(IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public async Task EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            var expression = await accessExpressionProvider.GetDeleteExpression(userContext);

            ThrowIfDenied<TIdentifier, TEntity>(expression, userContext, entities);
        }
    }

    extension<TIdentifier, TEntity>(IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> accessExpressionProvider)
    where TEntity : IEntity<TIdentifier>
    {
        public async Task EnsureAccess(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        {
            var expression = await accessExpressionProvider.GetUpdateExpression(userContext);

            ThrowIfDenied<TIdentifier, TEntity>(expression, userContext, entities);
        }
    }

    private static void ThrowIfDenied<TIdentifier, TEntity>(Expression<Func<TEntity, bool>> expression, UserContext userContext, IReadOnlyCollection<TEntity> entities)
    where TEntity : IEntity<TIdentifier>
    {
        var func = expression.Compile();

        var deniedAccess = entities.Where(entity => !func(entity)).ToList();

        if (deniedAccess.Count != 0)
        {
            throw new EntityAccessDeniedException<TIdentifier, TEntity>(userContext, deniedAccess.Select(d => d.Identifier));
        }
    }
}
