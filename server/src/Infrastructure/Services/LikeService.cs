using Application.Core;
using Application.DTOs.LikeDTOs;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class LikeService(ApplicationDbContext context, ISpamRepository spamRepository,
    IUserActionLogger<LikeService> logger) : ILikeService
{
    public async Task<Result<bool>> IsLikedByAsync(string userId, string publicationId, CancellationToken ct)
    {
        try
        {
            bool result = await context.Likes.AnyAsync(a => a.LikedById == userId
                && a.PublicationId == publicationId, ct);
            return Result<bool>.Success(result);
        }
        catch(Exception)
        {
            return Result<bool>.Failure("During getting info if the post is liked by an error happened", 500);
        }
    }

    public async Task<Result<LikeDto>> LikePublicationAsync(string publicationId, string userId, CancellationToken ct)
    {
            var postExists = await context.Publications.AnyAsync(a => a.Id == publicationId, ct);
            if (!postExists) return Result<LikeDto>.Failure("Post you are trying to like does not exist", 404);

            var userExists = await context.Users.AnyAsync(a => a.Id == userId, ct);
            if (!userExists) return Result<LikeDto>.Failure("Attempt to like a post of an unauthorized user is impossible", 403);

            var like = await context.Likes
                .FirstOrDefaultAsync(a => a.PublicationId == publicationId && a.LikedById == userId, 
                ct);

            bool alreadyLiked = like != null;
            bool isLikedByUser = false;

            if (!alreadyLiked)
            {
                var canLike = await spamRepository.MakeLike(userId, ct);
                if (!canLike)
                {
                    return Result<LikeDto>.Failure("You cannot like more for today due to our antispam rules", 400);
                }
                
                context.Likes.Add(new Like
                {
                    LikedById = userId,
                    PublicationId = publicationId
                });
                isLikedByUser = true;
            }
            else
            {
                context.Likes.Remove(like!);
            }

            bool success = await context.SaveChangesAsync(ct) > 0;
            
            if (success)
            {
                int countOfLikes = await context.Likes
                    .CountAsync(a => a.PublicationId == publicationId, ct);

                var logAction = alreadyLiked ? UserLogAction.DislikePublication : UserLogAction.LikePublication;
                
                await logger.LogAsync(userId, logAction, new
                {
                    info = $"User {userId} has set a like for publication {publicationId} to {!alreadyLiked}"
                }, publicationId, ct);
                
                return Result<LikeDto>.Success(new LikeDto
                {
                    AmountOfLikes = countOfLikes,
                    IsLikedByCurrentUser = isLikedByUser
                });
            }

            return Result<LikeDto>.Failure("Action was not registered in the Database", 500);
    }
}