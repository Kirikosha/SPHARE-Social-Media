namespace Application.Features.Recommendations.Queries;

using Application.Core;
using Application.Features.Publications.Queries;
using Application.Services.SubscriptionService;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRecommendations
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
        ISubscriptionService subscriptionService, IMediator mediator) 
        : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!await context.Users.AnyAsync(a => a.Id == request.UserId))
                return Result<List<PublicationDto>>.Failure("User was not found", 404);

            List<int> subscriptions;
            try
            {
                subscriptions = await subscriptionService.GetFollowingAsync(request.UserId);
            }
            catch(Exception ex)
            {
                return Result<List<PublicationDto>>.Failure("Followings were not found", 404);
            }

            List<PublicationDto> publications = [];

            foreach(var subscription in subscriptions)
            {
                var userPublicationsResult = await mediator
                    .Send(new GetPublicationsByUserId.Query { UserId = request.UserId });

                if (!userPublicationsResult.IsSuccess)
                {
                    return Result<List<PublicationDto>>.Failure(userPublicationsResult.Error, userPublicationsResult.Code);
                }
                publications.AddRange(userPublicationsResult.Value);
            }

            publications = publications.OrderByDescending(a => a.PostedAt).ThenByDescending(a => a.LikesAmount)
                .ToList();
            return Result<List<PublicationDto>>.Success(publications);
        }
    }
}
