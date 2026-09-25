using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Examples.Common.Domain.Models.Entities;

public enum AttendanceProbability 
{
    Medium,
    High,
    Low,
}

[HasIdentifier(typeof(AttendanceIdentifier))]
public partial record Attendance : IEntity<AttendanceIdentifier>
{
    public required partial AttendanceIdentifier Identifier { get; init; }
    
    public required AttendanceProbability Probability { get; init; }
}
