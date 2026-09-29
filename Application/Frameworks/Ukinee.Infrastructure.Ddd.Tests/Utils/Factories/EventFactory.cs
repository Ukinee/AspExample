using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

public static class EventFactory
{
    public static readonly Event Example1 = new Event {
        Identifier = EventIdentifier.Create(UserFactory.User1.Guid, new Guid("5da78553-9ca5-4794-ab59-33ef19ba51eb")),
        IsAvailableForPublicRead = true,
        HappensAt = default,
    };

    public static readonly Event Example2 = new Event {
        Identifier = EventIdentifier.Create(UserFactory.User2.Guid, new Guid("9e2e2ada-a5c8-4d0d-b3a0-d85622f30c98")),
        IsAvailableForPublicRead = false,
        HappensAt = default,
    };

    public static readonly Event Example3 = new Event {
        Identifier = EventIdentifier.Create(UserFactory.User2.Guid, new Guid("d54c8941-e372-4c77-bc4d-9fe54e2b7f1b")),
        IsAvailableForPublicRead = true,
        HappensAt = default,
    };
}
