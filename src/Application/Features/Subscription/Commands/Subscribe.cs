using Application.Core;
using Application.Services.SubscriptionService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Commands;
public class Subscribe
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required string FollowUserUniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool userExists = await context.Users.AnyAsync(a => a.Id == request.UserId, cancellationToken);
            if (!userExists) return Result<bool>.Failure("User does not exist", 404);

            var userToFollow = await context.Users
                .Where(x => x.UniqueNameIdentifier == request.FollowUserUniqueNameIdentifier).FirstOrDefaultAsync(cancellationToken);
            if (userToFollow == null) return Result<bool>.Failure("User you want to follow does not exist", 400);
            
            try
            {
                await subscriptionService.FollowAsync(request.UserId, userToFollow.Id);

                var followerCount = await subscriptionService.GetFollowersAsync(userToFollow.Id);
                userToFollow.SubscriberNumber = followerCount.Count;
                context.Users.Update(userToFollow);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message, 500);
            }
        }
    }
}
