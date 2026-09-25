using System.Linq.Expressions;
using System.Reflection;
using Ardalis.Specification;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.DbAccess.Extensions
{
    public static class QueryableIdentifierExtensions
    {
        public static IQueryable<TEntity> WhereIdEquals<TEntity, TIdentifier>(
            this IQueryable<TEntity> query,
            TIdentifier identifier
        )
        where TEntity : class, IEntity<TIdentifier>
        where TIdentifier : struct
        {
            if (typeof(TIdentifier).IsAssignableTo(typeof(IComplexIdentifier<TIdentifier>)))
            {
                var lambda = IdentifierExpressionCache<TEntity, TIdentifier>.CreateFilter(identifier);

                return query.Where(lambda);
            }

            return query.Where(e => e.Identifier.Equals(identifier));
        }

        public static ISpecificationBuilder<TEntity> WhereIdEquals<TEntity, TIdentifier>(
            this ISpecificationBuilder<TEntity> query,
            TIdentifier identifier
        )
        where TEntity : class, IEntity<TIdentifier>
        where TIdentifier : struct
        {
            if (typeof(TIdentifier).IsAssignableTo(typeof(IComplexIdentifier<TIdentifier>)))
            {
                var lambda = IdentifierExpressionCache<TEntity, TIdentifier>.CreateFilter(identifier);

                return query.Where(lambda);
            }

            return query.Where(e => e.Identifier.Equals(identifier));
        }

        public static IQueryable<TEntity> WhereIdIn<TEntity, TIdentifier>(
            this IQueryable<TEntity> query,
            IEnumerable<TIdentifier> identifiers
        )
        where TEntity : class, IEntity<TIdentifier>
        where TIdentifier : struct
        {
            if (typeof(TIdentifier).IsAssignableTo(typeof(IComplexIdentifier<TIdentifier>)))
            {
                var lambda = IdentifierExpressionCache<TEntity, TIdentifier>.CreateFilterForMany(identifiers);

                return query.Where(lambda);
            }

            return query.Where(e => identifiers.Contains(e.Identifier));
        }

        public static ISpecificationBuilder<TEntity> WhereIdIn<TEntity, TIdentifier>(
            this ISpecificationBuilder<TEntity> query,
            IEnumerable<TIdentifier> identifiers
        )
        where TEntity : class, IEntity<TIdentifier>
        where TIdentifier : struct
        {
            if (typeof(TIdentifier).IsAssignableTo(typeof(IComplexIdentifier<TIdentifier>)))
            {
                var lambda = IdentifierExpressionCache<TEntity, TIdentifier>.CreateFilterForMany(identifiers);

                return query.Where(lambda);
            }

            return query.Where(e => identifiers.Contains(e.Identifier));
        }
    }

    internal static class IdentifierExpressionCache<TEntity, TIdentifier>
    where TEntity : class
    where TIdentifier : struct
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly (PropertyInfo EntityProperty, PropertyInfo IdentifierProperty)[] MappedProperties;

        static IdentifierExpressionCache()
        {
            var identifierProperties = typeof(TIdentifier).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var entityType = typeof(TEntity);

            var list = new List<(PropertyInfo, PropertyInfo)>();

            foreach (var property in identifierProperties)
            {
                var entityProperty = entityType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);

                if (entityProperty == null)
                {
                    throw new InvalidOperationException(
                        $"Entity '{entityType.Name}' does not contain property '{property.Name}', " +
                        $"witch is expected by identifier type '{typeof(TIdentifier).Name}'."
                    );
                }

                list.Add((entityProperty, property));
            }

            MappedProperties = list.ToArray();
        }

        public static Expression<Func<TEntity, bool>> CreateFilter(TIdentifier identifier)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? body = null;

            foreach (var mapping in MappedProperties)
            {
                var entityProperty = Expression.Property(parameter, mapping.EntityProperty); // e => e.SomeProperty

                var value = mapping.IdentifierProperty.GetValue(identifier); // var value = identifier.SomeProperty; // "123"
                var constant = Expression.Constant(value, mapping.IdentifierProperty.PropertyType); // () => "123"

                var equality = Expression.Equal(entityProperty, constant); // e => e.SomeProperty == "123"

                body = body == null ? equality : Expression.AndAlso(body, equality);
            }

            if (body == null)
                return _ => false;

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter); // e => e.SomeProperty1 == "123" && e.SomeProperty2 == "321"
        }

        public static Expression<Func<TEntity, bool>> CreateFilterForMany(IEnumerable<TIdentifier> identifiers)
        {
            var idList = identifiers as IReadOnlyList<TIdentifier> ?? identifiers.ToList();

            if (idList.Count == 0)
                return _ => false;

            if (MappedProperties.Length == 1)
            {
                var mapping = MappedProperties[0];
                var propType = mapping.EntityProperty.PropertyType;

                var method = typeof(IdentifierExpressionCache<TEntity, TIdentifier>)
                    .GetMethod(nameof(BuildSinglePropertyContains), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(propType);

                return (Expression<Func<TEntity, bool>>)method.Invoke(null, [mapping.EntityProperty, mapping.IdentifierProperty, idList])!;
            }

            return BuildMultiplePropertiesContains(idList);
        }

        private static Expression<Func<TEntity, bool>> BuildMultiplePropertiesContains(IReadOnlyList<TIdentifier> idList) // todo: optimize
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? totalBody = null;

            foreach (var identifier in idList)
            {
                Expression? idBody = null;

                foreach (var mapping in MappedProperties)
                {
                    var entityProperty = Expression.Property(parameter, mapping.EntityProperty);
                    var value = mapping.IdentifierProperty.GetValue(identifier);
                    var constant = Expression.Constant(value, mapping.IdentifierProperty.PropertyType);
                    var equality = Expression.Equal(entityProperty, constant);

                    idBody = idBody == null ? equality : Expression.AndAlso(idBody, equality);
                }

                if (idBody != null)
                {
                    totalBody = totalBody == null ? idBody : Expression.OrElse(totalBody, idBody);
                }
            }

            if (totalBody == null)
                return _ => false;

            return Expression.Lambda<Func<TEntity, bool>>(totalBody, parameter); // e => (e.Property1 == "123" && e.Property2 == "321") || (e.Property1 == "abc" && e.Property2 == "cba")
        }

        private static Expression<Func<TEntity, bool>> BuildSinglePropertyContains<TSinglePropertyType>(
            PropertyInfo entityPropertyInfo,
            PropertyInfo identifierPropertyInfo,
            IEnumerable<TIdentifier> identifiers
        )
        {
            var values = identifiers.Select(id => (TSinglePropertyType)identifierPropertyInfo.GetValue(id)!).ToList();

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var entityProperty = Expression.Property(parameter, entityPropertyInfo); // e => e.SingleProperty

            var containsMethod = typeof(List<TSinglePropertyType>).GetMethod(nameof(List<>.Contains), [typeof(TSinglePropertyType)])!; // Contains(string)
            var constantsList = Expression.Constant(values); // () => ["123", "321", "111"]
            var body = Expression.Call(constantsList, containsMethod, entityProperty); // ["123", "321", "111"].Contains(e.SingleProperty)

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter); // e => ["123", "321", "111"].Contains(e.SingleProperty)
        }
    }
}
