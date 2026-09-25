using System.Linq.Expressions;
using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class AttendanceAccessExpressionProvider : ComplexEntityAccessExpressionProviderBase<AttendanceIdentifier, Attendance>
{
    public override async Task<Expression<Func<Attendance, bool>>> GetCreateExpression(UserContext userContext)
    {
        return LoggedInExpression(userContext);
    }

    public override async Task<Expression<Func<Attendance, bool>>> GetDeleteExpression(UserContext userContext)
    {
        return UserGuidInIdentifierExpression(nameof(AttendanceIdentifier.AttendeeUserGuid), userContext);
    }

    public override async Task<Expression<Func<Attendance, bool>>> GetUpdateExpression(UserContext userContext)
    {
        return UserGuidInIdentifierExpression(nameof(AttendanceIdentifier.AttendeeUserGuid), userContext);
    }

    public override async Task<Expression<Func<Attendance, bool>>> GetReadExpression(UserContext userContext)
    {
        var ownerRule = UserGuidInIdentifierExpression(nameof(AttendanceIdentifier.AttendeeUserGuid), userContext);
        var eventOwnerRule = UserGuidInIdentifierExpression(nameof(AttendanceIdentifier.EventUserGuid), userContext);

        return Or(ownerRule, eventOwnerRule);
    }
}
