namespace Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;

public record Host
{
    public string Endpoint => $"{Protocol}://{Hostname}:{Port}{BasePath}";

    public required string Protocol { get; init; }
    public required string Hostname { get; init; }
    public required int Port { get; init; }
    public required string BasePath { get; init; }
}
