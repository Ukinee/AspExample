namespace Ukinee.Infrastructure.Ddd.External.Api.Domain;

public interface IRouteParams
{
    public static abstract string RouteTemplate { get; }
    
    public string GetPath();
}
