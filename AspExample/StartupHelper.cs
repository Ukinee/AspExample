namespace AspExample;

public class ModeConfig
{
    public required bool IsServer { get; init; }
    public required int Port { get; init; }
}

public static class StartupHelper
{
    public static ModeConfig GetMode(string[] args)
    {
        const string ClientFlag = "client";
        const string ServerFlag = "server";

        var mode = args
            .FirstOrDefault(a => a.Equals($"-{ClientFlag}", StringComparison.OrdinalIgnoreCase)
                                 || a.Equals($"-{ServerFlag}", StringComparison.OrdinalIgnoreCase)
            )
            ?.TrimStart('-')
            .ToLowerInvariant();

        return mode switch {
            ClientFlag => GetClientConfig(),
            ServerFlag => GetServerConfig(),
            _ => throw new InvalidOperationException($"Usage: <app> -{ClientFlag} | -{ServerFlag}")
        };
    }

    public static ModeConfig GetServerConfig() =>
        new ModeConfig { IsServer = true, Port = 5000 };

    public static ModeConfig GetClientConfig() =>
        new ModeConfig { IsServer = false, Port = 5001 };
}
