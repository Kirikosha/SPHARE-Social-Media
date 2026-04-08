namespace Application.Features.Comments.Queries;
using Core;
using DTOs.CommentDTOs;
using Application.Interfaces.Services;
public class GetComment
{
    public class Query : IRequest<Result<CommentDto>>
    {
        public required string Id { get; init; }
    }

    public class Handler(ICommentService commentService) : IRequestHandler<Query, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await commentService.GetCommentByIdAsync(request.Id, cancellationToken);
        }
    }
}
