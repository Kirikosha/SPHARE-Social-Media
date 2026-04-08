namespace Application.Features.Publications.Queries;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationById
{
    public class Query : IRequest<Result<PublicationDto>>
    {
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(IPublicationService publicationService) 
        : IRequestHandler<Query, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService
                .GetPublicationByIdAsync(request.PublicationId, request.UserId, cancellationToken);
        }
    }
}
