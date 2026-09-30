namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public enum AuthorizedOperation
{
    Create,
    Read,
    Update,
    Delete,
}

public enum AccessLevel
{
    Guest,
    LoggedIn,
    Owner,
    Admin,
}