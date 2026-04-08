namespace Application.Features.Subscription.Queries;
using Core;
using Application.Interfaces.Services;
public class IsFollowing
{
    public class Query : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await subscriptionService.IsFollowing(request.UserId, request.UniqueNameIdentifier,
                cancellationToken);
        }
    }
}
