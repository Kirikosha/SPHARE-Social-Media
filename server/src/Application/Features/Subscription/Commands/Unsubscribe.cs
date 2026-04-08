namespace Application.Features.Subscription.Commands;
using Core;
using Application.Interfaces.Services;
public class Unsubscribe
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required string UnfollowUserUniqueNameIdentifier { get; set; }
    }

    public class Handler(ISubscriptionService subscriptionService)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await subscriptionService.Unsubscribe(request.UserId, request.UnfollowUserUniqueNameIdentifier,
                cancellationToken);
        }
    }
}
