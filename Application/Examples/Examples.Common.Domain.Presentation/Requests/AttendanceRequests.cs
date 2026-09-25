using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;

namespace Examples.Common.Domain.Presentation.Requests;

public class CreateAttendanceRequest
{
    public required EventIdentifier EventIdentifier { get; init; }
    public required AttendanceProbability AttendanceProbability { get; init; }
}

public record UpdateAttendanceProbabilityRequest
{
    public required AttendanceProbability AttendanceProbability { get; init; }
}
