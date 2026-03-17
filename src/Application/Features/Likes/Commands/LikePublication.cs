using Application.Repositories.SpamRepository;

namespace Application.Features.Likes.Commands;

using Core;
using Domain.DTOs.LikeDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class LikePublication
{
    public class Command : IRequest<Result<LikeDto>>
    {
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISpamRepository spamRepository) : IRequestHandler<Command, Result<LikeDto>>
    {
        public async Task<Result<LikeDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool isLikedByUser = false;

            var postExists = await context.Publications.AnyAsync(a => a.Id == request.PublicationId, cancellationToken);
            var userExists = await context.Users.AnyAsync(a => a.Id == request.UserId, cancellationToken);
            if (!postExists) return Result<LikeDto>.Failure("Post you are trying to like does not exist", 404);
            if (!userExists) return Result<LikeDto>.Failure("Attempt to like a post of an unauthorized user is impossible", 403);

            var like = await context.Likes.FirstOrDefaultAsync(a => a.PublicationId == request.PublicationId
            && a.LikedById == request.UserId, cancellationToken);

            var res = await spamRepository.MakeLike(request.UserId);
            if (res == "Forbidden")
            {
                return Result<LikeDto>.Failure(
                    "You cannot like more for today due to our antispam rules", 400);
            }
            
            if (like != null)
            {
                context.Likes.Remove(like);
            }
            else
            {
                await context.Likes.AddAsync(new Like
                {
                    LikedById = request.UserId,
                    PublicationId = request.PublicationId
                }, cancellationToken);
                isLikedByUser = true;
            }

            bool success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (success)
            {
                int countOfLikes = await context.Publications
                    .Include(a => a.Likes).Where(a => a.Id == request.PublicationId)
                    .Select(a => a.Likes.Count()).FirstOrDefaultAsync(cancellationToken);

                LikeDto result = new LikeDto
                {
                    AmountOfLikes = countOfLikes,
                    IsLikedByCurrentUser = isLikedByUser
                };
                return Result<LikeDto>.Success(result);
            }
            return Result<LikeDto>.Failure("Action was not registered in the Database", 500);
        }
    }
}
