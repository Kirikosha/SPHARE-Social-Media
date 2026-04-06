using Application.Core;
using Application.Interfaces.Services;
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
            try
            {
                bool followerExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
                if (!followerExists)
                    return Result<bool>.Failure("User does not exist", 404);
                
                var targetUserId = await context.Users
                    .Where(u => u.UniqueNameIdentifier == request.FollowUserUniqueNameIdentifier)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (targetUserId == null)
                    return Result<bool>.Failure("User you want to follow does not exist", 400);
            
                await subscriptionService.FollowAsync(request.UserId, targetUserId);

                int rowsUpdated = await context.Users
                    .Where(u => u.Id == targetUserId)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(u => u.SubscriberNumber, u => u.SubscriberNumber + 1),
                        cancellationToken);

                if (rowsUpdated == 0)
                {
                    await subscriptionService.UnfollowAsync(request.UserId, targetUserId);
                    return Result<bool>.Failure("User not found while updating count", 404);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message, 500);
            }
        }
    }
}
