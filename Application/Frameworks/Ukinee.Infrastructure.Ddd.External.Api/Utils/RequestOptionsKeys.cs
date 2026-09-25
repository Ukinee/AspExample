using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.External.Api.Utils;

public static class RequestOptionsKeys
{
    public static readonly HttpRequestOptionsKey<UserContext> UserContext = new("UserContext");
}