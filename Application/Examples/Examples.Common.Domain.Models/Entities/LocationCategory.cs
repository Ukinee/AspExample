using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Examples.Common.Domain.Models.Entities;

[HasIdentifier(typeof(LocationCategoryIdentifier))]
public partial record LocationCategory : IEntity<LocationCategoryIdentifier>, IEntityWithPublicRead<LocationCategory>
{
    public required partial LocationCategoryIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public LocationCategory OpenPublicRead()
    {
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public LocationCategory ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
