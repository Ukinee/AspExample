namespace Ukinee.Infrastructure.Validation.Exceptions;

public class ValidationFailedException : Exception
{
    public ValidationFailedException(string[] messages) : base($"Validation failed: {string.Join(", ", messages)}")
    {
        Messages = messages;
    }

    public string[] Messages { get; }
}
