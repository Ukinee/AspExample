using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Server.Domain.Models.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

public static class LocationFactory
{
    public static readonly Location Example1 = new Location {
        Identifier = LocationIdentifier.Create("AABBCC"),
        IsAvailableForPublicRead = true,
        Address = new Address("StreetA", "CityA", "StateA", "ZipA", "CountryA"),
    };

    public static readonly Location Example2 = new Location {
        Identifier = LocationIdentifier.Create("112233"),
        IsAvailableForPublicRead = false,
        Address = new Address("StreetB", "CityB", "StateB", "ZipB", "CountryB"),
    };

    public static readonly Location Example3 = new Location {
        Identifier = LocationIdentifier.Create("!!@@##"),
        IsAvailableForPublicRead = true,
        Address = new Address("StreetC", "CityC", "StateC", "ZipC", "CountryC"),
    };
}
