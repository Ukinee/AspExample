using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;

public class ExampleServerHub(IUserContextProvider userContextProvider, ISignalRConnectionStateStore connectionStateStore, HubSessionStore hubSessionStore)
    : HubBase(userContextProvider, connectionStateStore, hubSessionStore) { }

