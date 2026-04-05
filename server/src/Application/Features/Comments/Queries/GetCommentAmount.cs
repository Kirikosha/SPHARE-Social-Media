using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

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
            var amount = await context.Comments.CountAsync(a => a.PublicationId == request.Id, cancellationToken);
            return Result<int>.Success(amount);
        }
    }
}
