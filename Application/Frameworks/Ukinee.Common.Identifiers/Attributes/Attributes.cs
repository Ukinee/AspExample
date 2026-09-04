using System;

namespace Ukinee.Common.Identifiers.Attributes
{
    public interface IComplexIdentifier<TSelf> { }

    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class IdentifierAttribute : Attribute
    {
        public string[] Order { get; }

        public IdentifierAttribute(params string[] order) =>
            Order = order;
    }

    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class RouteParamsIdentifierAttribute : Attribute
    {
        public string[] Exclude { get; }

        public RouteParamsIdentifierAttribute(params string[] exclude) =>
            Exclude = exclude;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class HasIdentifierAttribute : Attribute
    {
        public Type IdentifierType { get; }

        public HasIdentifierAttribute(Type identifierType)
        {
            IdentifierType = identifierType;
        }
    }
}
