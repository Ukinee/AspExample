using Examples.Server.Domain.Models.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Databases;

public static class ApplyServerDomainConventionsExtension
{
    public static void ApplyServerDomainConventions(this ModelConfigurationBuilder builder)
    {
        builder.ComplexProperties<Address>();
        builder.ComplexProperties<UserContext>();
    }
}
