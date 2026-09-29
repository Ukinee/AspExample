using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using FluentValidation;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class LocationCategoryFactory() : IEntityCreateFactory<CreateLocationCategoryRequest, LocationCategory>
{
    public LocationCategory Create(UserContext userContext, CreateLocationCategoryRequest payload)
    {
        return new LocationCategory {
            Identifier = LocationCategoryIdentifier.Create(payload.LocationCategoryName),
            IsAvailableForPublicRead = false,
        };
    }
}

public class CreateLocationCategoryRequestValidator : AbstractValidator<IEnumerable<CreateLocationCategoryRequest>>
{
    public CreateLocationCategoryRequestValidator()
    {
    }
}
