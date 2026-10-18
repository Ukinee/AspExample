namespace Ukinee.Infrastructure.Ddd.External.Api.Domain;

public interface IRouteParams
{
    public string Path { get; }
    public static abstract string RouteTemplate { get; }
}
