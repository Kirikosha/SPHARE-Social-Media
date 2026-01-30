namespace Application.Features.Likes.Queries;

using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class IsLikedBy
{
    public class Query : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required int PublicationId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                bool result = await context.Likes.AnyAsync(a => a.LikedById == request.UserId
                && a.PublicationId == request.PublicationId);

                return Result<bool>.Success(result);
            }
            catch(Exception ex)
            {
                return Result<bool>.Failure("During getting info if the post is liked by an error happened", 500);
            }

        }
    }
}
