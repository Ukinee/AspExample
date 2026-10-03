using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

[Identifier(nameof(Guid))]
[RouteParamsIdentifier]
public readonly partial record struct BorderIdentifier
{
    public Guid Guid { get; init; }
}

public record CreateBorderRequest
{
    public required int Size { get; init; }
}

public record UpdateBorderRequest
{
    public required int Size { get; init; }
}

public record BorderResponse
{
    public required BorderIdentifier Identifier { get; init; }
    public required int Size { get; init; }
}

public record BorderSignalRRequest
{
}

[HasIdentifier(typeof(BorderIdentifier))]
public partial record Border : IEntity<BorderIdentifier>, IEntityWithPublicRead<Border>
{
    public required partial BorderIdentifier Identifier { get; init; }

    public required int Size { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public Border OpenPublicRead()
    {
        throw new NotImplementedException();
    }

    public Border ClosePublicRead()
    {
        throw new NotImplementedException();
    }
}
