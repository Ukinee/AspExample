using System.Collections.Concurrent;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class HubSessionStore
{
    private readonly ConcurrentDictionary<string, Action> _activeConnections = new();

    public void RegisterConnection(string connectionId, Action abortAction)
    {
        _activeConnections[connectionId] = abortAction;
    }

    public void UnregisterConnection(string connectionId)
    {
        _activeConnections.TryRemove(connectionId, out _);
    }

    public bool AbortConnection(string connectionId)
    {
        if (!_activeConnections.TryRemove(connectionId, out var abortAction))
            return false;

        abortAction.Invoke();

        return true;
    }
}
