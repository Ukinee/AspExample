using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

public class LocationCategoryFactory
{
    public static LocationCategory Example1 => new LocationCategory {
        Identifier = LocationCategoryIdentifier.Create("Map1"),
        IsAvailableForPublicRead = true,
    };
}
