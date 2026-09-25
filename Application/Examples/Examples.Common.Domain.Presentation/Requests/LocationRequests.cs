using Examples.Server.Domain.Models.ValueObjects;

namespace Examples.Common.Domain.Presentation.Requests;

public class CreateLocationRequest
{
    public required Address Address { get; init; }
}
