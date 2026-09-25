using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Domain.Contracts;

public interface IUserFactory
{
    public User Create(UserCredentials credentials);
    public User UpdateHash(User old, UserCredentials credentials);
}
