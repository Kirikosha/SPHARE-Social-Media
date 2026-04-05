using Application.Core;
using Application.Services.SubscriptionService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Queries;
public class IsFollowing
{
    public class Query : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool currentUserExists = await context.Users.AnyAsync(a => a.Id == request.UserId, cancellationToken);
            if (!currentUserExists) return Result<bool>.Failure("User does not exist", 404);

            string? otherUserId = await context.Users.Where(x => x.UniqueNameIdentifier == request.UniqueNameIdentifier)
                .Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (string.IsNullOrEmpty(otherUserId)) return Result<bool>.Failure("User, whom you wnat to check if you are " +
                "following does not " +
                "exist", 400);

            try
            {
                bool isFollowing = await subscriptionService.IsFollowing(request.UserId, otherUserId);
                return Result<bool>.Success(isFollowing);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message, 500);
            }
        }
    }
}
