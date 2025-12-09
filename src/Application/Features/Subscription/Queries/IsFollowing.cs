using Application.Core;
using Application.Services.SubscriptionService;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Queries;
public class IsFollowing
{
    public class Query : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService, IMapper mapper) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool currentUserExists = await context.Users.AnyAsync(a => a.Id == request.UserId);
            if (!currentUserExists) return Result<bool>.Failure("User does not exist", 404);

            int otherUserId = await context.Users.Where(x => x.UniqueNameIdentifier == request.UniqueNameIdentifier).Select(x => x.Id).FirstOrDefaultAsync();
            if (otherUserId == 0) return Result<bool>.Failure("User, whom you wnat to check if you are following does not exist", 400);

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
