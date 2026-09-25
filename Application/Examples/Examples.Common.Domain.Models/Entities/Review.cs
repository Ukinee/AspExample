using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Examples.Common.Domain.Models.Entities;

[HasIdentifier(typeof(ReviewIdentifier))]
public partial record Review : IEntity<ReviewIdentifier>, IEntityWithPublicRead<Review>, IEntityWithSoftDelete<Review>
{
    public required partial ReviewIdentifier Identifier { get; init; }

    public required bool IsAvailableForPublicRead { get; init; }

    public bool IsDeleted => DeletedAt.HasValue;

    public DateTimeOffset? DeletedAt { get; private init; }

    public Review Delete(DateTimeOffset now)
    {
        return this with {
            DeletedAt = now,
        };
    }

    public Review OpenPublicRead()
    {
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public Review ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
