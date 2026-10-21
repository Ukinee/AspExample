using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;

public class TestingUserContextProvider(UserContext userContext) : IUserContextProvider
{
    public UserContext GetActiveUserContext() =>
        userContext;
}
