using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;

public interface IEntityAccessValidator<TEntity>;

public interface IIdentifierEntityAccessValidator<TIdentifier, TEntity> : IEntityAccessValidator<TEntity>
where TEntity : IEntity<TIdentifier>
{
    public Expression<Func<TEntity, bool>> GetExpression(UserContext userContext);

    public bool HasAccess(UserContext userContext, TIdentifier identifier);
    public void EnsureHasAccess(UserContext userContext, IEnumerable<TIdentifier> identifiers);

    public (IReadOnlyCollection<TIdentifier> AllowedAccess, IReadOnlyCollection<TIdentifier> DeniedAccess) Separate(UserContext userContext, IEnumerable<TIdentifier> identifiers);
}

public static class EntityAccessValidatorExtensions
{
    extension<TIdentifier, TEntity>(IIdentifierEntityAccessValidator<TIdentifier, TEntity> validator)
    where TEntity : IEntity<TIdentifier>
    {
        public void EnsureHasAccess(UserContext userContext, TIdentifier identifier) =>
            validator.EnsureHasAccess(userContext, [identifier]);

        public void EnsureHasAccess(UserContext userContext, TEntity entity) =>
            validator.EnsureHasAccess(userContext, [entity.Identifier]);

        public void EnsureHasAccess(UserContext userContext, IEnumerable<TEntity> entities) =>
            validator.EnsureHasAccess(userContext, entities.Select(x => x.Identifier));
    }
}
