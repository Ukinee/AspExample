using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.DbAccess.Extensions;

public static class IdentifierExtensions
{
    public static EntityTypeBuilder<TEntity> ConfigureComplexIdentifierKey<TIdentifier, TEntity>(this EntityTypeBuilder<TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, ISpanParsable<TIdentifier>, IComplexIdentifier<TIdentifier>
    {
        var keyProperties = typeof(TIdentifier)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        if (keyProperties.Length > 0)
        {
            builder.HasKey(keyProperties);
        }

        builder.Ignore(nameof(IEntity<>.Identifier));

        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureIdentifierKey<TIdentifier, TEntity>(this EntityTypeBuilder<TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, ISpanParsable<TIdentifier>
    {
        if (typeof(TIdentifier).IsAssignableTo(typeof(IComplexIdentifier<TIdentifier>)))
        {
            throw new InvalidOperationException($"{nameof(ConfigureIdentifierKey)} cannot be used with {typeof(IComplexIdentifier<TIdentifier>)}. Use {nameof(ConfigureComplexIdentifierKey)} instead.");
        }

        builder.HasKey(p => p.Identifier);

        return builder;
    }
}
