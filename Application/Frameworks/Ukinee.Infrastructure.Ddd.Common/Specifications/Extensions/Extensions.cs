using System.Linq.Expressions;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Utils;

namespace Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;

public static class Specification
{
    public static ISpecificationBuilder<T> For<T>()
    {
        return new Specification<T>().Query;
    }
}

public static class SpecificationExtensions
{
    extension<TEntity>(ISpecificationBuilder<TEntity> specification)
    {
        public ISpecificationBuilder<TEntity> OptionalWhere(Expression<Func<TEntity, bool>>? predicate)
        {
            return predicate == null ? specification : specification.Where(predicate);
        }

        public ISpecificationBuilder<TEntity> Exists()
        {
            return specification.Where(DddExpressionFactory.Exists<TEntity>());
        }
    }
}
