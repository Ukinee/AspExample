using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

public static class ReviewFactory
{
    public static readonly Review Example1 = new Review {
        Identifier = ReviewIdentifier.Create(
            EventFactory.Example2.EventGuid,
            EventFactory.Example2.UserGuid,
            UserFactory.User1.Guid,
            new Guid("00e2353a-96ba-4d55-b18b-9430e656b0bf")
        ),
        IsAvailableForPublicRead = true,
    };

    public static readonly Review Example2 = new Review {
        Identifier = ReviewIdentifier.Create(
            EventFactory.Example1.EventGuid,
            EventFactory.Example1.UserGuid,
            UserFactory.User2.Guid,
            new Guid("2f5cb845-cf32-4e47-89fd-c917b830a98f")
        ),
        IsAvailableForPublicRead = false,
    };

    public static readonly Review Example3 = new Review {
        Identifier = ReviewIdentifier.Create(
            EventFactory.Example3.EventGuid,
            EventFactory.Example3.UserGuid,
            UserFactory.User1.Guid,
            new Guid("2f5cb845-cf32-4e47-89fd-c917b830a98f")
        ),
        IsAvailableForPublicRead = true,
    };
}
