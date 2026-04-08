namespace Application.Features.Publications.Queries;
using Core.Pagination;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUniqueNameIdentifier
{
    public class Query : IRequest<Result<PagedList<PublicationCardDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
        public required string UserId { get; set; }
        public PaginationParams PaginationParams { get; set; } = new();
    }

    public class Handler(IPublicationService publicationService) 
        : IRequestHandler<Query, Result<PagedList<PublicationCardDto>>>
    {
        public async Task<Result<PagedList<PublicationCardDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService.GetPublicationsByUniqueNameIdentifierAsync(request.UniqueNameIdentifier,
                request.UserId, request.PaginationParams.PageNumber, request.PaginationParams.PageSize,
                cancellationToken);
        }
    }
}
