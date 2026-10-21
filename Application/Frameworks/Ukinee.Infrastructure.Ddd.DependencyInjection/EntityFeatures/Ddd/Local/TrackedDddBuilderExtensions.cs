using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public static class TrackedDddBuilderExtensions
{
    extension<TTag, TIdentifier, TEntity>(TrackedDddBuilder<TTag, TIdentifier, TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct
    {
    }
}
