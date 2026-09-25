namespace Ukinee.Infrastructure.Common.Utils;

public class PathHelper
{
    public static void EnsurePathExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (directory == null)
        {
            throw new DirectoryNotFoundException(filePath);
        }

        var directoryInfo = new DirectoryInfo(directory);

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
    }

    public static void EnsureDirectoryExists(string directory)
    {
        var directoryInfo = new DirectoryInfo(directory);

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
    }
}
