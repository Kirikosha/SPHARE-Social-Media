namespace Application.Features.Comments.Queries;
using Core;
using Core.Pagination;
using DTOs.CommentDTOs;
using Application.Interfaces.Services;
public class GetReplies
{
    public class Query : IRequest<Result<PagedList<CommentDto>>>
    {
        public required string ParentId { get; set; }
        public PaginationParams Pagination { get; init; } = new();
    }

    public class Handler(ICommentService commentService) : IRequestHandler<Query, Result<PagedList<CommentDto>>>
    {
        public async Task<Result<PagedList<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await commentService.GetRepliesAsync(request.ParentId, request.Pagination.PageNumber, request
                .Pagination.PageSize, cancellationToken);
        }
    }
}