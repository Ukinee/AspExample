using MediatR;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

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

public record PayloadRequest<TPayload> : IRequest
{
    public required UserContext UserContext { get; init; }
    public required IReadOnlyCollection<TPayload> Contents { get; init; }
}

public record PayloadRequest<TPayload, TResponse> : IRequest<IReadOnlyCollection<TResponse>>
{
    public required UserContext UserContext { get; init; }
    public required IReadOnlyCollection<TPayload> Contents { get; init; }
}
