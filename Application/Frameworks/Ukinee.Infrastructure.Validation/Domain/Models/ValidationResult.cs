using Ukinee.Infrastructure.Validation.Exceptions;

namespace Ukinee.Infrastructure.Validation.Domain.Models;

public record ValidationResult
{
    public static ValidationResult CreateValid() => new ValidationResult()
    {
        Status = ValidationStatus.Valid,
    };

    public static ValidationResult CreateInvalid(string message) => new ValidationResult()
    {
        Status = ValidationStatus.Invalid,
        Messages = new[] { message }
    };

    public static ValidationResult CreateInvalid(IEnumerable<string> messages) => new ValidationResult()
    {
        Status = ValidationStatus.Invalid,
        Messages = messages.ToArray()
    };

    public ValidationStatus Status { get; init; } = ValidationStatus.NotYetValidated;

    public string[] Messages { get; init; } = Array.Empty<string>();

    public void EnsureValid()
    {
        if(Status != ValidationStatus.Valid)
            throw new ValidationFailedException(Messages);
    }
}
