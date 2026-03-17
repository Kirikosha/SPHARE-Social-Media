using Application.Core;
using Application.Services.SubscriptionService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Queries;
public class GetFollowersCount
{
    public class Query : IRequest<Result<int>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            string? userId = await context.Users.Where(x => x.UniqueNameIdentifier == request.UniqueNameIdentifier)
                .Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(userId)) return Result<int>.Failure("Such user was not found", 400);

            try
            {
                var result = await subscriptionService.GetFollowersAsync(userId);
                return Result<int>.Success(result.Count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message, 500);
            }
        }
    }
}
