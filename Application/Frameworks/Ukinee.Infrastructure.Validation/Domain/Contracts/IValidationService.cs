using System.Diagnostics.Contracts;
using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Domain.Contracts;

public interface IValidationService<in T>
{
    [Pure]
    public ValidationResult Validate(T model);
}
