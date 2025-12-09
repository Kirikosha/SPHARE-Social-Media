using Application.Core;
using Application.Services.SubscriptionService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Commands;
public class Subscribe
{
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required string FollowUserUniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool userExists = await context.Users.AnyAsync(a => a.Id == request.UserId);
            if (!userExists) return Result<bool>.Failure("User does not exist", 404);

            int userToFollowId = await context.Users.Where(x => x.UniqueNameIdentifier == request.FollowUserUniqueNameIdentifier).Select(x => x.Id).FirstOrDefaultAsync();
            if (userToFollowId == 0) return Result<bool>.Failure("User you want to follow does not exist", 400);

            try
            {
                await subscriptionService.FollowAsync(request.UserId, userToFollowId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message, 500);
            }
        }
    }
}
