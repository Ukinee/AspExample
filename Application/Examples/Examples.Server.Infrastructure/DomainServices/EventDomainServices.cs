using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.Validation.Services;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class EventFactory :
    IEntityCreateFactory<CreateEventRequest, Event>,
    IEntityUpdateFactory<UpdateEventDateRequest, Event>
{
    public Event Create(UserContext userContext, CreateEventRequest payload)
    {
        return new Event {
            Identifier = EventIdentifier.New(userContext),
            IsAvailableForPublicRead = false,
            HappensAt = payload.HappensAt,
        };
    }

    public Event Update(UserContext userContext, Event old, UpdateEventDateRequest payload)
    {
        return old with {
            HappensAt = payload.HappensAt,
        };
    }
}

public class CreateEventRequestValidator : ValidationServiceBase<CreateEventRequest>
{
    public CreateEventRequestValidator() { }
}

public class UpdateEventDateRequestValidator : ValidationServiceBase<UpdateEventDateRequest>
{
    public UpdateEventDateRequestValidator() { }
}
