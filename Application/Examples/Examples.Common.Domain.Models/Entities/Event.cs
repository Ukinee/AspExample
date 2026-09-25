using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Examples.Common.Domain.Models.Entities;

[HasIdentifier(typeof(EventIdentifier))]
public partial record Event : IEntity<EventIdentifier>, IEntityWithPublicRead<Event>
{
    public required partial EventIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }
    
    public required DateTimeOffset HappensAt { get; init; }

    public Event OpenPublicRead()
    {
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public Event ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
