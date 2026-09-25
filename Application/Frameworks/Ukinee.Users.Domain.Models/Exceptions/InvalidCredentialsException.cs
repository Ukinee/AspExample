namespace Ukinee.Users.Domain.Exceptions;

public class InvalidCredentialsException : InvalidOperationException
{
    public InvalidCredentialsException( string username)
    {
        Username = username;
    }

    public string Username { get; }
}
