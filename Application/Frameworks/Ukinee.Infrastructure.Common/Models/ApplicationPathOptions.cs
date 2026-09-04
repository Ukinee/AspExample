namespace Ukinee.Infrastructure.Common.Models;

public sealed class ApplicationPathOptions
{
    public required string FullPath { get; init; }

    public string GetAppDataFolder()
    {
        return FullPath;
    }
}
