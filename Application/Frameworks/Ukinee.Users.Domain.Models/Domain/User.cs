using System.Collections.Immutable;
using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Common.Identifiers.Attributes;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Users.Domain;

[HasIdentifier(typeof(UserIdentifier))]
public partial record User : IEntity<UserIdentifier>
{
    public required partial UserIdentifier Identifier { get; init; }

    public required ImmutableArray<string> Roles { get; init; }
    public required UserAccount Account { get; init; }

    public static User Dummy => field ??= new User {
        Identifier = UserIdentifier.Create(Guid.AllBitsSet),
        Roles = [],
        Account = new UserAccount {
            Username = "dummy",
            PasswordHash = string.Empty,
            CreatedAt = DateTimeOffset.MinValue,
            UpdatedAt = DateTimeOffset.MinValue,
        },
    };
}

public record UserAccount
{
    public required string Username { get; init; }
    public required string PasswordHash { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
