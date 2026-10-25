using MediatR;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Domain;

public record UserContextChangedNotification : INotification
{
    public required UserContext OldContext { get; init; }
    public required UserContext NewContext { get; init; }
}
