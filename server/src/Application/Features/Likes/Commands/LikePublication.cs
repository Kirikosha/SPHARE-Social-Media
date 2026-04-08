namespace Application.Features.Likes.Commands;
using DTOs.LikeDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class LikePublication
{
    public class Command : IRequest<Result<LikeDto>>
    {
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(ILikeService likeService) : 
        IRequestHandler<Command, Result<LikeDto>>
    {
        public async Task<Result<LikeDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await likeService.LikePublicationAsync(request.PublicationId, request.UserId, cancellationToken);
        }
    }
}
