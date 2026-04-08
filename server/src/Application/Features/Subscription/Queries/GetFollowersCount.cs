namespace Application.Features.Subscription.Queries;
using Core;
using Application.Interfaces.Services;
public class GetFollowersCount
{
    public class Query : IRequest<Result<int>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await subscriptionService.GetFollowersCount(request.UniqueNameIdentifier, cancellationToken);
        }
    }
}
