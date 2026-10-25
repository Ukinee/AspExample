using System.Collections.Concurrent;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Domain;

public class SignalRConnectionState
{
    public required string ConnectionId { get; init; }
    public required UserContext UserContext { get; init; }
    public required DateTimeOffset ConnectedAt { get; init; }
    
    public DateTimeOffset LastActivityAt { get; init; }

    public required ConcurrentDictionary<Guid, HashSet<string>> GroupsByMode { get; init; } = [];
}
