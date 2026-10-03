using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;

public class TestingUserContextProvider() : IUserContextProvider
{
    public UserContext UserContext { get; set; }
    
    public UserContext GetActiveUserContext() =>
        UserContext;
}
