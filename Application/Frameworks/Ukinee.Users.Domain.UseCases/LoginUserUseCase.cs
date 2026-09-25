using Microsoft.AspNetCore.Identity;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Domain.Exceptions;
using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Domain.UseCases;

public class LoginUserUseCase(
    IUserReader userReader,
    IUserCredentialsUpdater userCredentialsUpdater,
    IUserTokenFactory userTokenFactory,
    IPasswordHasher<User> passwordHasher
)
{
    private readonly string _dummyHash = passwordHasher.HashPassword(User.Dummy, "cool_dummy_password_11><23");

    public async Task<string> Execute(UserCredentials userCredentials, CancellationToken cancellationToken)
    {
        var existingUser = await userReader.FindByUsernameAsync(userCredentials.Username, cancellationToken);

        if (existingUser == null)
        {
            passwordHasher.VerifyHashedPassword(User.Dummy, _dummyHash, userCredentials.Password);

            throw new InvalidCredentialsException(userCredentials.Username);
        }

        var result = passwordHasher.VerifyHashedPassword(existingUser, existingUser.Account.PasswordHash, userCredentials.Password);

        return result switch {
            PasswordVerificationResult.Success => CreateToken(existingUser),
            PasswordVerificationResult.SuccessRehashNeeded => await RehashAndCreateToken(existingUser, userCredentials, cancellationToken),
            PasswordVerificationResult.Failed => throw new InvalidCredentialsException(userCredentials.Username),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string CreateToken(User user)
    {
        return userTokenFactory.Generate(user);
    }

    private async Task<string> RehashAndCreateToken(User user, UserCredentials credentials, CancellationToken cancellationToken)
    {
        var updatedUser = await userCredentialsUpdater.Rehash(user, credentials, cancellationToken);

        return userTokenFactory.Generate(updatedUser);
    }
}
