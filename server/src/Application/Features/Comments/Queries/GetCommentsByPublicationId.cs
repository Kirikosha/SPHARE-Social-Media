namespace Application.Features.Comments.Queries;
using Core.Pagination;
using DTOs.CommentDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class GetCommentsByPublicationId
{
    public class Query : IRequest<Result<PagedList<CommentDto>>>
    {
        public required string PublicationId { get; init; }
        public PaginationParams Pagination { get; init; } = new();
    }
    public class Handler(ICommentService commentService) : IRequestHandler<Query, Result<PagedList<CommentDto>>>
    {
        public async Task<Result<PagedList<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await commentService.GetCommentsByPublicationIdAsync(request.PublicationId, 
                request.Pagination.PageNumber, request.Pagination.PageSize, cancellationToken);
        }
    }
}
