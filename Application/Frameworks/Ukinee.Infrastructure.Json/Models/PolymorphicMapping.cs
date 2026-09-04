namespace Ukinee.Infrastructure.Json.Models;

public record PolymorphicMapping(Type BaseType, Type DerivedType, string DiscriminatorKey, string DiscriminatorValue);
