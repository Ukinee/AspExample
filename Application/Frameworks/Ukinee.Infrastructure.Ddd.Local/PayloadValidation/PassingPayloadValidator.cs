using FluentValidation;

namespace Ukinee.Infrastructure.Ddd.Local.PayloadValidation;

public class PassingPayloadValidator<TPayload> : AbstractValidator<IEnumerable<TPayload>>
{
    public PassingPayloadValidator()
    {
        // no-op
    }
}
