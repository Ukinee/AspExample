using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Examples.Server.Domain.Models.Contracts;
using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class LocationFactory(IAddressHashService hashService) : IEntityCreateFactory<CreateLocationRequest, Location>
{
    public Location Create(UserContext userContext, CreateLocationRequest payload)
    {
        var hash = hashService.Calculate(payload.Address);

        return new Location {
            Identifier = LocationIdentifier.Create(hash),
            Address = payload.Address,
            IsAvailableForPublicRead = false,
        };
    }
}

public class CreateLocationRequestValidator : AbstractValidator<IEnumerable<CreateLocationRequest>>
{
    public CreateLocationRequestValidator() { }
}
