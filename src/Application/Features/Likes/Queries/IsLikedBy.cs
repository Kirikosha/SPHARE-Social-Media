namespace Application.Features.Likes.Queries;

using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class IsLikedBy
{
    public class Query : IRequest<bool>
    {
        public required int UserId { get; set; }
        public required int PublicationId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, bool>
    {
        public async Task<bool> Handle(Query request, CancellationToken cancellationToken)
        {
            return await context.Likes.AnyAsync(a => a.LikedById == request.UserId
            && a.PublicationId == request.PublicationId);
        }
    }
}
