namespace Application.Features.Recommendations.Queries;

using Core;
using Application.Features.Publications.Queries;
using Services.SubscriptionService;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRecommendations
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required string UserId { get; set; }
    }

    public class Handler() 
        : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Result<List<PublicationDto>>.Success([]);
        }
    }
}
