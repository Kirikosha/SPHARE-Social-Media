namespace Application.Features.Likes.Queries;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class IsLikedBy
{
    public class Query : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required string PublicationId { get; set; }
    }

    public class Handler(ILikeService likeService) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await likeService.IsLikedByAsync(request.UserId, request.PublicationId, cancellationToken);
        }
    }
}
