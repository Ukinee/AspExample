using Microsoft.AspNetCore.SignalR;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.SignalR.Contracts;

public interface IHubService<in TRequest>
{
    public Task Subscribe(Hub hub, UserContext userContext, TRequest request);
    public Task Unsubscribe(Hub hub, UserContext userContext, TRequest request);
}
