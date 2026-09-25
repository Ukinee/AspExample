using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Examples.Server.Domain.Models;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Infrastructure.Validation.Services;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Examples.Server.Infrastructure.DomainServices;

public class ReviewFactory : IEntityCreateFactory<CreateReviewRequest, Review>
{
    public Review Create(UserContext userContext, CreateReviewRequest payload)
    {
        return new Review {
            Identifier = ReviewIdentifier.New(userContext, payload.EventIdentifier),
            IsAvailableForPublicRead = true,
        };
    }
}

public class CreateReviewRequestValidator : ValidationServiceBase<CreateReviewRequest>
{
    public CreateReviewRequestValidator() { }
}

public class SubscribeToReviewsRequestGroupResolver : RouteResolverBase<ServerExampleTag, Review, SubscribeToReviewsRequest>
{
    public SubscribeToReviewsRequestGroupResolver() { }
}

public class ReviewSignalRAccessValidatorService(IGetEntityUseCase<EventIdentifier, Event> getEventUseCase) : ISignalRAccessValidator<SubscribeToReviewsRequest>
{
    public async Task<bool> ValidateAccess(SubscribeToReviewsRequest request, UserContext userContext)
    {
        var readEntity = await getEventUseCase.ExecuteSoft(userContext, [request.EventIdentifier], CancellationToken.None).FirstOrDefaultAsync();
        
        return readEntity != null;
    }
}
