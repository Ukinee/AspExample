using Microsoft.AspNetCore.SignalR;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface IHubService<in TRequest>
{
    public Task Subscribe(Hub hub, UserContext userContext, TRequest request);
    public Task Unsubscribe(Hub hub, UserContext userContext, TRequest request);
}
