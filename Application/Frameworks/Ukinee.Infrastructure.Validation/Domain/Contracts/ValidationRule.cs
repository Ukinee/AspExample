using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Domain.Contracts;

public interface IValidationRule
{
    public ValidationStatus Validate(object model, out string message);
}

public abstract class ValidationRule<T> : IValidationRule
{
    ValidationStatus IValidationRule.Validate(object model, out string message)
    {
        if (model is not T concreteModel)
        {
            message = $"Model ({model?.GetType().Name ?? "null"}) is not of type {typeof(T).Name}";
            return ValidationStatus.Invalid;
        }
        
        return Validate(concreteModel, out message);
    }

    public abstract ValidationStatus Validate(T model, out string message);
}
