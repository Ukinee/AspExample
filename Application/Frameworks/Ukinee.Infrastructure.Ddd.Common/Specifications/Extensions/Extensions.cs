using System.Linq.Expressions;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

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
            if (!typeof(TEntity).IsAssignableTo(typeof(ISpecificationForSoftDelete<TEntity>)))
            {
                return specification;
            }
            
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, nameof(ISpecificationForSoftDelete<>.DeletedAt));
            var condition = Expression.Equal(property, Expression.Constant(default(DateTimeOffset)));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);

            return specification.Where(lambda);
        }
    }
}
