using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Validation.Services;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class AttendanceFactory :
    IEntityCreateFactory<CreateAttendanceRequest, Attendance>,
    IEntityUpdateFactory<UpdateAttendanceProbabilityRequest, Attendance>
{
    public Attendance Create(UserContext userContext, CreateAttendanceRequest payload)
    {
        return new Attendance {
            Identifier = AttendanceIdentifier.New(userContext, payload.EventIdentifier),
            Probability = payload.AttendanceProbability,
        };
    }

    public Attendance Update(UserContext userContext, Attendance old, UpdateAttendanceProbabilityRequest payload)
    {
        return old with {
            Probability = payload.AttendanceProbability,
        };
    }
}

public class CreateAttendanceRequestValidator : ValidationServiceBase<CreateAttendanceRequest>
{
    public CreateAttendanceRequestValidator() { }
}

public class UpdateAttendanceProbabilityRequestValidator : ValidationServiceBase<UpdateAttendanceProbabilityRequest>
{
    public UpdateAttendanceProbabilityRequestValidator() { }
}
