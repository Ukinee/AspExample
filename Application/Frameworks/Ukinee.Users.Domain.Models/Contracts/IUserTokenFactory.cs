namespace Ukinee.Users.Domain.Contracts;

public interface IUserTokenFactory
{
    public string Generate(User user);
}
