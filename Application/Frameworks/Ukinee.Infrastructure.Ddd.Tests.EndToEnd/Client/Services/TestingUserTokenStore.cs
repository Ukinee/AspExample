using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;

public class TestingUserTokenStore : IUserTokenStore
{
    private readonly Dictionary<Guid, string> _store = [];

    public string? GetToken(UserContext userContext)
    {
        return _store.GetValueOrDefault(userContext.Guid);
    }

    public void SetToken(UserContext userContext, string token)
    {
        _store[userContext.Guid] = token;
    }
}
