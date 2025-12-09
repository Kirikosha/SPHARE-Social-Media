using Application.Core;
using Application.Services.SubscriptionService;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subscription.Queries;
public class GetSubscriptions
{
    public class Query : IRequest<Result<List<PublicUserDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISubscriptionService subscriptionService, IMapper mapper) : IRequestHandler<Query, Result<List<PublicUserDto>>>
    {
        public async Task<Result<List<PublicUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            int userId = await context.Users.Where(x => x.UniqueNameIdentifier == request.UniqueNameIdentifier).Select(x => x.Id).FirstOrDefaultAsync();
            if (userId == 0) return Result<List<PublicUserDto>>.Failure("User, for whom we are looking for all the subscriptions, does not exist", 400);

            try
            {
                var subscriptedUsersIds = await subscriptionService.GetFollowingAsync(userId);
                var users = await context.Users.Include(a => a.ProfileImage)
                    .Where(a => subscriptedUsersIds.Contains(a.Id))
                    .ProjectTo<PublicUserDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
                return Result<List<PublicUserDto>>.Success(users);
            }
            catch (Exception ex)
            {
                return Result<List<PublicUserDto>>.Failure(ex.Message, 500);
            }
        }
    }
}
