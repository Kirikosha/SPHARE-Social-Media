using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Domain.Enums;

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

    public class Handler(ApplicationDbContext context, ISpamRepository spamRepository, 
        IUserActionLogger<LikePublication> _logger) : 
        IRequestHandler<Command, Result<LikeDto>>
    {
        public async Task<Result<LikeDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var postExists = await context.Publications.AnyAsync(a => a.Id == request.PublicationId, cancellationToken);
            if (!postExists) return Result<LikeDto>.Failure("Post you are trying to like does not exist", 404);

            var userExists = await context.Users.AnyAsync(a => a.Id == request.UserId, cancellationToken);
            if (!userExists) return Result<LikeDto>.Failure("Attempt to like a post of an unauthorized user is impossible", 403);

            var like = await context.Likes
                .FirstOrDefaultAsync(a => a.PublicationId == request.PublicationId && a.LikedById == request.UserId, cancellationToken);

            bool alreadyLiked = like != null;
            bool isLikedByUser = false;

            if (!alreadyLiked)
            {
                var canLike = await spamRepository.MakeLike(request.UserId);
                if (!canLike)
                {
                    return Result<LikeDto>.Failure("You cannot like more for today due to our antispam rules", 400);
                }
                
                context.Likes.Add(new Like
                {
                    LikedById = request.UserId,
                    PublicationId = request.PublicationId
                });
                isLikedByUser = true;
            }
            else
            {
                context.Likes.Remove(like!);
            }

            bool success = await context.SaveChangesAsync(cancellationToken) > 0;
            
            if (success)
            {
                int countOfLikes = await context.Likes
                    .CountAsync(a => a.PublicationId == request.PublicationId, cancellationToken);

                var logAction = alreadyLiked ? UserLogAction.DislikePublication : UserLogAction.LikePublication;
                
                await _logger.LogAsync(request.UserId, logAction, new
                {
                    info = $"User {request.UserId} has set a like for publication {request.PublicationId} to {!alreadyLiked}"
                }, request.PublicationId);
                
                return Result<LikeDto>.Success(new LikeDto
                {
                    AmountOfLikes = countOfLikes,
                    IsLikedByCurrentUser = isLikedByUser
                });
            }

            return Result<LikeDto>.Failure("Action was not registered in the Database", 500);
        }
    }
}
