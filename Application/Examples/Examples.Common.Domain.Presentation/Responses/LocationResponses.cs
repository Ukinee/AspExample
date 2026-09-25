using Examples.Server.Domain.Models.ValueObjects;

namespace Examples.Common.Domain.Presentation.Responses;

public class LocationResponse
{
    public required Address Address { get; init; }
}
