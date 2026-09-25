using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Users.Infrastructure.Implementations;

public class TestingUserContextProvider : IUserContextProvider
{
    public UserContext GetActiveUserContext()
    {
        return UserContext.Test;
    }
}
