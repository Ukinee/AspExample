using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local;

public static class TrackedDddBuilderExtensions
{
    extension<TTag, TIdentifier, TEntity>(ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct
    {
        public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithUserGuidInEntityIdentifierAccessValidator(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
        {
            return builder
                .WithIdentifierAccessValidator(sp =>
                    {
                        var reporter = sp.GetRequiredService<IAccessViolationReporter<TIdentifier, TEntity>>();

                        return new IdentifierEntityAccessValidator<TIdentifier, TEntity>(identifierUserGuid, reporter);
                    }
                );
        }

        public TrackedDddBuilder<TTag, TIdentifier, TEntity> WithPassingAccessValidator()
        {
            return builder
                .WithIdentifierAccessValidator<PassingIdentifierEntityAccessValidator<TIdentifier, TEntity>>();
        }
    }
}
