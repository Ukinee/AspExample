using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Users.Domain.Contracts;

public interface IUserContextProvider
{
    public UserContext GetActiveUserContext();
}
