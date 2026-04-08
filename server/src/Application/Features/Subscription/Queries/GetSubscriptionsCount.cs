using Application.Core;
using Application.Interfaces.Services;

namespace Application.Features.Subscription.Queries;
public class GetSubscriptionsCount
{
    public class Query : IRequest<Result<int>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await subscriptionService.GetSubscriptionsCount(request.UniqueNameIdentifier, cancellationToken);
        }
    }
}
