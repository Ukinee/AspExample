using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

public class ExampleCookieFactory
{
    public static Cookie Example1 => new Cookie {
        Identifier = CookieIdentifier.Create(ExampleUserFactory.User1, "Kappa1"),
        Calories = 123,
        IsAvailableForPublicRead = true,
    };

    public static Cookie Example2 => new Cookie {
        Identifier = CookieIdentifier.Create(ExampleUserFactory.User2, "Kappa2"),
        Calories = 321,
        IsAvailableForPublicRead = false,
    };

    public static Cookie Example3 => new Cookie {
        Identifier = CookieIdentifier.Create(ExampleUserFactory.User3, "Kappa3"),
        Calories = 999,
        IsAvailableForPublicRead = true,
    };

    public static CreateCookieRequest CreateRequest1 => new CreateCookieRequest {
        Calories = 123
    };
    
    public static UpdateCookieRequest UpdateRequest1 => new UpdateCookieRequest() {
        Calories = 9123
    };
}
