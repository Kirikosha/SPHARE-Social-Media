namespace Application.Features.Recommendations.Queries;

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
    public class Query : IRequest<List<PublicationDto>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
        SubscriptionService subscriptionService, IMediator mediator) 
        : IRequestHandler<Query, List<PublicationDto>>
    {
        public async Task<List<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!await context.Users.AnyAsync(a => a.Id == request.UserId))
            {
                throw new Exception("User not found");
            }

            List<int> subscriptions;
            try
            {
                subscriptions = await subscriptionService.GetFollowingAsync(request.UserId);
            }
            catch(Exception ex)
            {
                throw new Exception("Following was not found");
            }

            List<PublicationDto> publications = [];

            foreach(var subscription in subscriptions)
            {
                var userPublications = await mediator
                    .Send(new GetPublicationsByUserId.Query { UserId = request.UserId });

                publications.AddRange(userPublications);
            }

            publications = publications.OrderByDescending(a => a.PostedAt).ThenByDescending(a => a.LikesAmount)
                .ToList();
            return publications;
        }
    }
}
