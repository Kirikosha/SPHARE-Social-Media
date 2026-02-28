using Application.Core;
using Application.Services.SubscriptionService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Commands;
public class Unsubscribe
{
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required string UnfollowUserUniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool userExists = await context.Users.AnyAsync(a => a.Id == request.UserId, cancellationToken);
            if (!userExists) return Result<bool>.Failure("User does not exist", 404);

            var userToUnfollow = await context.Users.Where(x => x.UniqueNameIdentifier == request.UnfollowUserUniqueNameIdentifier).FirstOrDefaultAsync(cancellationToken);
            if (userToUnfollow == null) return Result<bool>.Failure("User you want to follow does not exist", 400);

            try
            {
                await subscriptionService.UnfollowAsync(request.UserId, userToUnfollow.Id);
                var subscriberAmount = await subscriptionService.GetFollowersAsync(userToUnfollow.Id);
                userToUnfollow.SubscriberNumber = subscriberAmount.Count;

                context.Users.Update(userToUnfollow);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message, 500);
            }
        }
    }
}
