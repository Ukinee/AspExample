using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Users.Domain.Exceptions;

public class UserAlreadyExistsException : InvalidOperationException
{
    public UserAlreadyExistsException(string username) : base($"User '{username}' already exists")
    {
        
    }
}
