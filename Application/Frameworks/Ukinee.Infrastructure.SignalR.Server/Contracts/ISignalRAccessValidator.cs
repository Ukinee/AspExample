using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface ISignalRAccessValidator<in TRequest>
{
    Task<bool> ValidateAccess(TRequest request, UserContext userContext);
}
