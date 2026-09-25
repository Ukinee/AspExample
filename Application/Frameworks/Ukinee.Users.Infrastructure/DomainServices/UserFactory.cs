using Examples.Common.Domain.Models.Identifiers;
using Microsoft.AspNetCore.Identity;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Infrastructure.DomainServices;

public class UserFactory(TimeProvider timeProvider, IPasswordHasher<User> passwordHasher) : IUserFactory
{
    public User Create(UserCredentials payload)
    {
        var user = new User {
            Identifier = UserIdentifier.New(),
            Roles = [],
            Account = new UserAccount {
                Username = payload.Username,
                PasswordHash = string.Empty,
                CreatedAt = timeProvider.GetUtcNow(),
                UpdatedAt = timeProvider.GetUtcNow(),
            },
        };

        var hashedPassword = passwordHasher.HashPassword(user, payload.Password);

        return user with {
            Account = user.Account with {
                PasswordHash = hashedPassword,
            },
        };
    }

    public User UpdateHash(User old, UserCredentials payload)
    {
        var hashedPassword = passwordHasher.HashPassword(old, payload.Password);

        return old with {
            Account = old.Account with {
                PasswordHash = hashedPassword,
                UpdatedAt = timeProvider.GetUtcNow(),
            },
        };
    }
}
