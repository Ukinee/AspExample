using Microsoft.AspNetCore.SignalR;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface IHubService<in TRequest>
where TRequest : ISignalRSubscribeRequest
{
    public Task Subscribe(Hub hub, UserContext userContext, TRequest request);
    public Task Unsubscribe(Hub hub, UserContext userContext, Guid requestGuid);
}
