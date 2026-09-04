using System.Collections.Immutable;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.DbAccess.Utils;

namespace Ukinee.DbAccess.Extensions;

public static class ApplyIdentifierConvention
{
    public static void ApplyIdentifierConverters(this ModelConfigurationBuilder builder, params Assembly[] assemblies)
    {
        var strongIdTypes = assemblies
            .Distinct()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsValueType && !t.IsEnum && IsStrongId(t))
            .ToList();

        foreach (var type in strongIdTypes)
        {
            RegisterStrongIdConverter(builder, type);
            RegisterNullableStrongIdConverter(builder, type);

            RegisterCollectionConverter(builder, type.MakeArrayType(), type);
            RegisterCollectionConverter(builder, typeof(List<>).MakeGenericType(type), type);

            RegisterCollectionConverter(builder, typeof(ImmutableArray<>).MakeGenericType(type), type);
            RegisterCollectionConverter(builder, typeof(ImmutableList<>).MakeGenericType(type), type);
        }
    }

    private static bool IsStrongId(Type type)
    {
        return type.CustomAttributes.Any(a => a.AttributeType == typeof(IdentifierAttribute));
    }

    private static void RegisterStrongIdConverter(ModelConfigurationBuilder builder, Type identifierType)
    {
        var propertiesMethod = typeof(ModelConfigurationBuilder)
            .GetMethods()
            .First(m => m.Name == nameof(ModelConfigurationBuilder.Properties)
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 0
            );

        var genericProperties = propertiesMethod.MakeGenericMethod(identifierType);
        var propertiesBuilder = genericProperties.Invoke(builder, null); // builder.Properties<identifierType>()

        var hasConversionMethod = propertiesBuilder!
            .GetType()
            .GetMethod(nameof(PropertiesConfigurationBuilder<>.HaveConversion), [typeof(Type)]);  // builder.Properties<identifierType>().HaveConversion

        var converterType = typeof(IdentifierToStringConverter<>).MakeGenericType(identifierType); // StrongIdToStringConverter<identifierType>
        hasConversionMethod!.Invoke(propertiesBuilder, [converterType]); //builder.Properties<identifierType>().HaveConversion(StrongIdToStringConverter<identifierType)
    }

    private static void RegisterNullableStrongIdConverter(ModelConfigurationBuilder builder, Type type)
    {
        var nullableType = typeof(Nullable<>).MakeGenericType(type);

        var propertiesMethod = typeof(ModelConfigurationBuilder)
            .GetMethods()
            .First(m => m.Name == nameof(ModelConfigurationBuilder.Properties)
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 0
            );

        var genericProperties = propertiesMethod.MakeGenericMethod(nullableType);
        var propertiesBuilder = genericProperties.Invoke(builder, null);

        var hasConversionMethod = propertiesBuilder!
            .GetType()
            .GetMethod(nameof(PropertiesConfigurationBuilder<>.HaveConversion), [typeof(Type)]);

        var converterType = typeof(NullableIdentifierToStringConverter<>).MakeGenericType(type);
        hasConversionMethod!.Invoke(propertiesBuilder, [converterType]);
    }

    private static void RegisterCollectionConverter(ModelConfigurationBuilder builder, Type collectionType, Type identifierType)
    {
        var propertiesMethod = typeof(ModelConfigurationBuilder)
            .GetMethods()
            .First(m => m.Name == nameof(ModelConfigurationBuilder.Properties)
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 0
            );
        
        var genericProperties = propertiesMethod.MakeGenericMethod(collectionType); 
        var propertiesBuilder = genericProperties.Invoke(builder, null); // ModelConfigurationBuilder.Properties<collectionType>();

        var hasConversionMethod = propertiesBuilder!
            .GetType()
            .GetMethod(nameof(PropertiesConfigurationBuilder<>.HaveConversion), [typeof(Type), typeof(Type)]);

        var converterType = typeof(IdentifierCollectionConverter<,>).MakeGenericType(collectionType, identifierType);
        var comparerType = typeof(IdentifierCollectionComparer<,>).MakeGenericType(collectionType, identifierType);

        hasConversionMethod!.Invoke(propertiesBuilder, [converterType, comparerType]);
    }
}

public class IdentifierToStringConverter<TIdentifier> : ValueConverter<TIdentifier, string>
where TIdentifier : struct, ISpanParsable<TIdentifier>
{
    public IdentifierToStringConverter()
        : base(
            id => id.ToString()!,
            str => DbAccessUtils.Parse<TIdentifier>(str)
        ) { }
}

public class NullableIdentifierToStringConverter<TIdentifier> : ValueConverter<TIdentifier?, string?>
where TIdentifier : struct, ISpanParsable<TIdentifier>
{
    public NullableIdentifierToStringConverter()
        : base(
            v => v.HasValue ? v.Value.ToString() : null,
            s => s == null ? null : DbAccessUtils.Parse<TIdentifier>(s)
        ) { }
}

public class IdentifierCollectionConverter<TCollection, TIdentifier> : ValueConverter<TCollection, string[]>
where TCollection : IEnumerable<TIdentifier>
where TIdentifier : struct, ISpanParsable<TIdentifier>
{
    public IdentifierCollectionConverter()
        : base(
            c => IsDefaultOrEmpty(c) ? Array.Empty<string>() : c.Select(id => id.ToString()!).ToArray(),
            s => s == null ? CreateCollection(Array.Empty<TIdentifier>()) : CreateCollection(s.Select(DbAccessUtils.Parse<TIdentifier>))
        ) { }

    public static bool IsDefaultOrEmpty(TCollection? c)
    {
        if (c is null)
            return true;

        var type = c.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
        {
            var isDefaultProp = type.GetProperty("IsDefault");

            if (isDefaultProp != null && (bool)isDefaultProp.GetValue(c)!)
            {
                return true;
            }
        }

        return !c.Any();
    }

    public static TCollection CreateCollection(IEnumerable<TIdentifier> items)
    {
        Type t = typeof(TCollection);

        if (t.IsArray)
            return (TCollection)(object)items.ToArray();

        if (!t.IsGenericType)
            return (TCollection)(object)items.ToList();

        var def = t.GetGenericTypeDefinition();

        if (def == typeof(ImmutableArray<>))
            return (TCollection)(object)ImmutableArray.CreateRange(items);

        if (def == typeof(ImmutableList<>))
            return (TCollection)(object)ImmutableList.CreateRange(items);

        if (def == typeof(ImmutableHashSet<>))
            return (TCollection)(object)ImmutableHashSet.CreateRange(items);

        if (def == typeof(IImmutableList<>))
            return (TCollection)(object)ImmutableList.CreateRange(items);

        return (TCollection)(object)items.ToList();
    }
}

public class IdentifierCollectionComparer<TCollection, TIdentifier> : ValueComparer<TCollection>
where TCollection : IEnumerable<TIdentifier> 
where TIdentifier : struct, ISpanParsable<TIdentifier>
{
    public IdentifierCollectionComparer()
        : base(
            (c1, c2) => EqualsInternal(c1, c2),
            c => GetHashCodeInternal(c),
            c => CreateSnapshot(c)
        ) { }

    private static bool EqualsInternal(TCollection? c1, TCollection? c2)
    {
        bool isDefault1 = IdentifierCollectionConverter<TCollection, TIdentifier>.IsDefaultOrEmpty(c1);
        bool isDefault2 = IdentifierCollectionConverter<TCollection, TIdentifier>.IsDefaultOrEmpty(c2);

        if (isDefault1 && isDefault2)
            return true;

        if (isDefault1 || isDefault2)
            return false;

        return c1!.SequenceEqual(c2!);
    }

    private static int GetHashCodeInternal(TCollection? c)
    {
        if (IdentifierCollectionConverter<TCollection, TIdentifier>.IsDefaultOrEmpty(c))
            return 0;

        return c!.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()));
    }

    private static TCollection CreateSnapshot(TCollection? source)
    {
        if (IdentifierCollectionConverter<TCollection, TIdentifier>.IsDefaultOrEmpty(source))
        {
            return IdentifierCollectionConverter<TCollection, TIdentifier>.CreateCollection(Array.Empty<TIdentifier>());
        }

        return IdentifierCollectionConverter<TCollection, TIdentifier>.CreateCollection(source!);
    }
}
