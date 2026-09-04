using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;

public interface IAccessViolationReporter<in TIdentifier, in TEntity>
where TIdentifier : notnull
where TEntity : IEntity<TIdentifier>
{
    public void ReportViolation(UserContext userContext, TEntity entity);
    public void ReportViolation(UserContext userContext, TIdentifier identifier);

    public void ReportViolation(UserContext userContext, IEnumerable<TIdentifier> identifiers);
    public void ReportViolation(UserContext userContext, IEnumerable<TEntity> entities);
}
