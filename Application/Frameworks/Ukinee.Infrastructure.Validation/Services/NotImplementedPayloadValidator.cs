using Ukinee.Infrastructure.Validation.Domain.Contracts;
using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Services;

public class NotImplementedPayloadValidator<TPayload>
    : IValidationService<TPayload>
{
    public ValidationResult Validate(TPayload model)
    {
        throw new NotImplementedException();
    }
}