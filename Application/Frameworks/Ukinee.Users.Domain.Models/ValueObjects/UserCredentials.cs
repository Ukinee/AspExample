namespace Ukinee.Users.Domain.ValueObjects;

public readonly record struct UserCredentials
{
    public required string Password { get; init; }
    public required string Username { get; init; }

    public override string ToString() =>
        $"{nameof(UserCredentials)} {{ {nameof(Username)}: {Username} }})";
}
