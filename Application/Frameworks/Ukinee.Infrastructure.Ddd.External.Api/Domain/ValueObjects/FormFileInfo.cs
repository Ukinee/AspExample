namespace Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;

public class FormFileInfo
{
    public required Stream File { get; init; }
    public required string FileName { get; init; }
}
