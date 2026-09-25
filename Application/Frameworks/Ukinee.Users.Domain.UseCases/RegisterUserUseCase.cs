using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Domain.Exceptions;
using Ukinee.Users.Domain.ValueObjects;

namespace Ukinee.Users.Domain.UseCases;

public class RegisterUserUseCase(IUserReader reader, IUserCreator creator)
{
    public async Task<User> Execute(UserCredentials credentials, CancellationToken cancellationToken)
    {
        var existing = await reader.FindByUsernameAsync(credentials.Username, cancellationToken);

        if (existing != null)
            throw new UserAlreadyExistsException(credentials.Username);

        return await creator.Create(credentials, cancellationToken);
    }
}
