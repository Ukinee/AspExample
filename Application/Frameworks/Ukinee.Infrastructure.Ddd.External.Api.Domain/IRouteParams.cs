namespace Ukinee.Infrastructure.Ddd.External.Api.Domain;

public interface IRouteParams<out TSelf, in TIdentifier>
{
    public string Path { get; }
    public static abstract string RouteTemplate { get; }
    
    public static abstract TSelf FromIdentifier(TIdentifier identifier);
}
