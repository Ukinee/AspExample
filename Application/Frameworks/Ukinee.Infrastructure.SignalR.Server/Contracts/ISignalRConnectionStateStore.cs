using System.Diagnostics.CodeAnalysis;
using Ukinee.Infrastructure.SignalR.Server.Domain;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface ISignalRConnectionStateStore
{
    public SignalRConnectionState GetOrCreate(string connectionId, UserContext ctx);

    public bool TryGet(string connectionId, [NotNullWhen(true)] out SignalRConnectionState? state);
    public void Remove(string connectionId);

    public bool TryGet(Guid userContextGuid, out IReadOnlyCollection<SignalRConnectionState> state);
}
