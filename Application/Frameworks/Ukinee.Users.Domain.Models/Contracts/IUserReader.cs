using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Domain.Contracts;

public interface IUserReader
{
    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}

public interface IUserCredentialsUpdater
{
    public Task<User> Rehash(User user, UserCredentials credentials, CancellationToken cancellationToken);
}

public interface IUserCreator
{
    public Task<User> Create(UserCredentials credentials, CancellationToken cancellationToken);
}
