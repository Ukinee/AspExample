namespace Ukinee.Infrastructure.Ddd.Common.Entities;

public record SearchByIdRequest<TIdentifier>
{
    public required List<TIdentifier> Identifiers { get; init; }
}

public record GetOrCreateRequest<TIdentifier, TPayload>
{
    public required TIdentifier Identifier { get; init; }
    public required TPayload Payload { get; init; }
}

public record UpdateEntityRequest<TIdentifier, TPayload>
{
    public required TIdentifier Identifier { get; init; }
    public required TPayload Payload { get; init; }
}
