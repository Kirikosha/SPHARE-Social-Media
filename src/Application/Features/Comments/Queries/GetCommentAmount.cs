using Application.Core;
using Infrastructure;

namespace Application.Features.Comments.Queries;
public class GetCommentAmount
{
    public class Query : IRequest<Result<int>>
    {
        public required string Id { get; init; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            var amount = context.Comments.Count(a => a.PublicationId == request.Id);
            return Result<int>.Success(amount);
        }
    }
}
