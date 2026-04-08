namespace Application.Features.Publications.Queries;
using Core;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;

public class GetPlannedPublications
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required string UserId { get; set; }
    }
    
    public class Handler(IPublicationService publicationService) : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService.GetPlannedPublicationsAsync(request.UserId, cancellationToken);
        }
    }
}