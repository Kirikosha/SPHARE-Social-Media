
namespace Application.Features.Recommendations.Queries;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetRecommendations
{
    public class Query : IRequest<Result<List<PublicationCardDto>>>
    {
        public required string UserId { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
    }

    public class Handler(IRecommendationService recommendationService)
        : IRequestHandler<Query, Result<List<PublicationCardDto>>>
    {
        public async Task<Result<List<PublicationCardDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await recommendationService.GetRecommendations(request.UserId, request.Page, request.PageSize,
                cancellationToken);
        }

    }
}
