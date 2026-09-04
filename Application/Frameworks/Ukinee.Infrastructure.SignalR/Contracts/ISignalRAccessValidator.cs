using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.SignalR.Contracts;

public interface ISignalRAccessValidator<in TRequest>
{
    Task<bool> ValidateAccess(TRequest request, UserContext userContext);
}
