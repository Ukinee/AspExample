using System.Linq.Expressions;
using Ukinee.Infrastructure.Validation.Domain.Contracts;
using Ukinee.Infrastructure.Validation.Domain.Models;

namespace Ukinee.Infrastructure.Validation.Services;

public class ValidationServiceBase<T> : IValidationService<T>
{
    private readonly IReadOnlyList<ValidationRule<T>> _modelRules;
    private readonly IReadOnlyList<PropertyValidationInfo> _propertyRules;

    protected ValidationServiceBase(
        IReadOnlyList<ValidationRule<T>> modelRules,
        IReadOnlyList<PropertyValidationInfo> propertyRules
    )
    {
        _modelRules = modelRules ?? new List<ValidationRule<T>>();
        _propertyRules = propertyRules ?? new List<PropertyValidationInfo>();
    }

    public ValidationResult Validate(T value)
    {
        var status = ValidationStatus.Valid;
        List<string> messages = new List<string>();

        foreach (var info in _propertyRules)
        {
            var currentStatus = info.Validate(value!, out string message);

            if (currentStatus == ValidationStatus.Invalid)
            {
                messages.Add($"[{info.PropertyName}] {message}");
                status = currentStatus;
            }
        }

        foreach (var rule in _modelRules)
        {
            var currentStatus = rule.Validate(value, out string message);

            if (currentStatus == ValidationStatus.Invalid)
            {
                messages.Add(message);
                status = currentStatus;
            }
        }

        if (status == ValidationStatus.Valid)
            return ValidationResult.CreateValid();

        return ValidationResult.CreateInvalid(messages);
    }

    protected static PropertyValidationInfo Rule<TPropertyType>(Expression<Func<T, TPropertyType>> propertySelector, ValidationRule<TPropertyType> rule)
    {
        return new PropertyValidationInfo<TPropertyType>(propertySelector, rule);
    }

    protected class PropertyValidationInfo<TPropertyType> : PropertyValidationInfo
    {
        private readonly Func<T, TPropertyType> _propertySelectorFunc;

        private readonly ValidationRule<TPropertyType> _rule;

        public PropertyValidationInfo(Expression<Func<T, TPropertyType>> propertySelector, ValidationRule<TPropertyType> rule)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));

            if (propertySelector is null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            _propertySelectorFunc = propertySelector.Compile();
            PropertyName = ((MemberExpression)propertySelector.Body).Member.Name;
        }

        public override string PropertyName { get; }

        public override ValidationStatus Validate(object value, out string message)
        {
            if (value is not T concreteType)
            {
                throw new ArgumentException($"'{nameof(value)}' must be of type '{typeof(TPropertyType).Name}'.");
            }
            
            return Validate(concreteType, out message);
        }

        public ValidationStatus Validate(T value, out string message)
        {
            var propertyValue = _propertySelectorFunc(value);

            return _rule.Validate(propertyValue, out message);
        }
    }
    
    protected abstract class PropertyValidationInfo
    {
        public abstract string PropertyName { get; }
        public abstract ValidationStatus Validate(object value, out string message);
    }
}
