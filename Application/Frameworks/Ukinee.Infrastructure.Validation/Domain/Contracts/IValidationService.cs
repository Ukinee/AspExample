using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Domain.Contracts;

public interface IValidationService<in T>
{
    public ValidationResult Validate(T model);
}
