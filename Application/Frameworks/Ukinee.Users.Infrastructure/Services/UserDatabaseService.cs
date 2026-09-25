using Microsoft.EntityFrameworkCore;
using Ukinee.Users.Databases;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Infrastructure.Services;

public class UserDatabaseService(IUserFactory userFactory, UserDbContext userDbContext) : IUserReader, IUserCredentialsUpdater, IUserCreator
{
    public async Task<User> Create(UserCredentials credentials, CancellationToken cancellationToken)
    {
        var user = userFactory.Create(credentials);

        await userDbContext.Users.AddAsync(user, cancellationToken);
        await userDbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return userDbContext.Users.AsNoTracking().Where(u => u.Account.Username == username).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> Rehash(User user, UserCredentials credentials, CancellationToken cancellationToken)
    {
        var updatedUser = userFactory.UpdateHash(user, credentials);

        userDbContext.Users.Update(updatedUser);
        await userDbContext.SaveChangesAsync(cancellationToken);

        return updatedUser;
    }
}
