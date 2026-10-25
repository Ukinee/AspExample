using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Domain;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class LocalSignalRConnectionStateStore(TimeProvider timeProvider) : ISignalRConnectionStateStore
{
    private readonly ConcurrentDictionary<string, SignalRConnectionState> _connectionStates = [];

    public SignalRConnectionState GetOrCreate(string connectionId, UserContext ctx)
    {
        var now = timeProvider.GetUtcNow();

        return _connectionStates.GetOrAdd(
            connectionId,
            (id) => new SignalRConnectionState {
                ConnectionId = id,
                UserContext = ctx,
                ConnectedAt = now,
                LastActivityAt = now,
                GroupsByMode = [],
            }
        );
    }

    public bool TryGet(string connectionId, [NotNullWhen(true)] out SignalRConnectionState? state)
    {
        return _connectionStates.TryGetValue(connectionId, out state);
    }

    public void Remove(string connectionId)
    {
        _connectionStates.TryRemove(connectionId, out _);
    }

    public bool TryGet(Guid userContextGuid, out IReadOnlyCollection<SignalRConnectionState> state)
    {
        var result = new List<SignalRConnectionState>();

        foreach (var pair in _connectionStates)
        {
            if (pair.Value.UserContext.Guid != userContextGuid)
                continue;

            result.Add(pair.Value);
        }

        state = result;

        return result.Count > 0;
    }
}
