using Examples.Common.Domain.Models.Identifiers;

namespace Examples.Common.Domain.Presentation.Responses;

public class AttendanceResponse
{
    public required AttendanceIdentifier Identifier { get; init; }
}
