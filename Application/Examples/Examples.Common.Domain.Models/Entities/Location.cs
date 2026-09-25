using Examples.Common.Domain.Models.Identifiers;
using Examples.Server.Domain.Models.ValueObjects;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Examples.Common.Domain.Models.Entities;

[HasIdentifier(typeof(LocationIdentifier))]
public partial record Location : IEntity<LocationIdentifier>, IEntityWithPublicRead<Location>
{
    public required partial LocationIdentifier Identifier { get; init; }

    public required bool IsAvailableForPublicRead { get; init; }

    public required Address Address { get; init; }

    public Location OpenPublicRead()
    {
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public Location ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
