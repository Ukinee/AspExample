using System.Collections.Immutable;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

public interface IEntityWithSharedAccess<out TSelf>
{
    public ImmutableArray<Guid> SharedUserGuids { get; }

    public TSelf AddSharedUser(UserContext userContext);
    public TSelf RemoveSharedUser(UserContext userContext);
}
