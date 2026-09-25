using Examples.Common.Domain.Models.Identifiers;

namespace Examples.Common.Domain.Presentation.Requests;

public class CreateReviewRequest
{
    public required EventIdentifier EventIdentifier { get; init; }
    public required string Content { get; init; }
}

public record SubscribeToReviewsRequest
{
    public required EventIdentifier EventIdentifier { get; init; }
}
