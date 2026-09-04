using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Validation.Domain.Contracts;
using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Services;

public class PassingValidator<TPayload>(ILogger<PassingValidator<TPayload>> logger)
    : IValidationService<TPayload>
{
    public ValidationResult Validate(TPayload model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Use of PassingValidator for type {type}", typeof(TPayload));
        }

        return ValidationResult.CreateValid();
    }
}
