
namespace Application.Features.Subscription.Queries;
using Core;
using DTOs.UserDTOs;
using Application.Interfaces.Services;
public class GetSubscriptions
{
    public class Query : IRequest<Result<List<PublicUserDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<List<PublicUserDto>>>
    {
        public async Task<Result<List<PublicUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await subscriptionService.GetSubscriptions(request.UniqueNameIdentifier, cancellationToken);
        }
    }
}
