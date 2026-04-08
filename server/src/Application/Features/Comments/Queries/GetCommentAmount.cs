namespace Application.Features.Comments.Queries;
using Core;
using Application.Interfaces.Services;
public class GetCommentAmount
{
    public class Query : IRequest<Result<int>>
    {
        public required string PublicationId { get; init; }
    }

    public class Handler(ICommentService commentService) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await commentService.GetCommentAmountAsync(request.PublicationId, cancellationToken);
        }
    }
}
