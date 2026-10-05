using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Contracts;

public interface IUserTokenStore
{
    public string? GetToken(UserContext userContext);
    
    public void SetToken(UserContext userContext, string token);
}
