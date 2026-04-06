using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Domain.DTOs.UserDTOs;
using Domain.Enums;

namespace Application.Features.Comments.Commands;

using Core;
using Domain.DTOs.CommentDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class CreateComment
{
    private const int CommentRestrictionInSeconds = 10;
    public class Command : IRequest<Result<CommentDto>>
    {
        public required string UserId { get; set; }
        public required CreateCommentDto Comment { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISpamRepository spamRepository, 
        IUserActionLogger<CreateComment> logger) : IRequestHandler<Command, Result<CommentDto>>
    {
        private const int MaxDepth = 10;

        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userInfo = await GetUserInfoAsync(request, cancellationToken);

            if (userInfo == null)
                return Result<CommentDto>.Failure("User was not found", 400);

            if (!userInfo.PublicationExists)
                return Result<CommentDto>.Failure("Publication you are trying to comment on no longer exists", 400);

            var cutOff = DateTime.UtcNow.AddSeconds(-CommentRestrictionInSeconds);
            if (userInfo.LastCommentDate > cutOff)
                return Result<CommentDto>.Failure("You are sending comments too fast", 400);

            var spamResult = await spamRepository.MakeComment(request.UserId);
            if (!spamResult)
                return Result<CommentDto>.Failure("You cannot make comments for today due to our antispam rules", 400);

            var parentValidation = await ValidateParentAsync(request, cancellationToken);
            if (!parentValidation.IsSuccess)
                return Result<CommentDto>.Failure(parentValidation.Error, parentValidation.Code);

            var effectiveParentId = await ResolveEffectiveParentIdAsync(
                request.Comment.ParentCommentId, cancellationToken);

            var comment = new Comment
            {
                AuthorId = request.UserId,
                PublicationId = request.Comment.PublicationId,
                Content = request.Comment.Content,
                CreationDate = DateTime.UtcNow,
                ParentCommentId = effectiveParentId
            };

            await PersistCommentAsync(comment, effectiveParentId, cancellationToken);

            await logger.LogAsync(request.UserId, UserLogAction.CreateComment, new
            {
                info = $"Comment {comment.Id} was created by user {request.UserId}"
            }, comment.PublicationId, cancellationToken);

            return Result<CommentDto>.Success(new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreationDate = comment.CreationDate,
                PublicationId = comment.PublicationId,
                IsDeleted = false,
                RepliesAmount = 0,
                Author = new PublicUserBriefDto
                {
                    Id = userInfo.Id,
                    Username = userInfo.Username,
                    UniqueNameIdentifier = userInfo.UniqueNameIdentifier,
                    Blocked = userInfo.Blocked,
                    ImageUrl = userInfo.ImageUrl
                }
            });
        }

        private async Task<UserInfoProjection?> GetUserInfoAsync(
            Command request, CancellationToken cancellationToken)
        {
            return await context.Users
                .Where(u => u.Id == request.UserId)
                .Select(u => new UserInfoProjection
                {
                    Id = u.Id,
                    Username = u.Username,
                    UniqueNameIdentifier = u.UniqueNameIdentifier,
                    Blocked = u.Blocked,
                    ImageUrl = u.ProfileImage == null ? null : u.ProfileImage.ImageUrl,
                    LastCommentDate = context.Comments
                        .Where(c => c.AuthorId == u.Id)
                        .OrderByDescending(c => c.CreationDate)
                        .Select(c => c.CreationDate)
                        .FirstOrDefault(),
                    PublicationExists = context.Publications
                        .Any(p => p.Id == request.Comment.PublicationId)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<Result<bool>> ValidateParentAsync(
            Command request, CancellationToken cancellationToken)
        {
            if (request.Comment.ParentCommentId == null)
                return Result<bool>.Success(true);

            var snapshot = request.Comment.ParentCommentId;
            var parentInfo = await context.Comments
                .Where(c => c.Id == snapshot)
                .Select(c => new { c.PublicationId })
                .FirstOrDefaultAsync(cancellationToken);

            if (parentInfo == null)
                return Result<bool>.Failure("Parent comment you are responding to no longer exists", 400);

            if (parentInfo.PublicationId != request.Comment.PublicationId)
                return Result<bool>.Failure("Comment reply internal server error. Try again later!", 500);

            return Result<bool>.Success(true);
        }

        private async Task<string?> ResolveEffectiveParentIdAsync(
            string? parentId, CancellationToken cancellationToken)
        {
            if (parentId == null) return null;

            int depth = 0;
            string? currentId = parentId;

            while (currentId != null)
            {
                var snapshot = currentId;
                var parent = await context.Comments
                    .Where(c => c.Id == snapshot)
                    .Select(c => new { c.ParentCommentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (parent?.ParentCommentId == null) break;
                depth++;
                currentId = parent.ParentCommentId;
            }

            if (depth + 1 <= MaxDepth)
                return parentId;

            int stepsUp = depth - (MaxDepth - 1);
            currentId = parentId;

            for (int i = 0; i < stepsUp; i++)
            {
                var snapshot = currentId;
                currentId = await context.Comments
                    .Where(c => c.Id == snapshot)
                    .Select(c => c.ParentCommentId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return currentId;
        }

        private async Task PersistCommentAsync(
            Comment comment, string? effectiveParentId, CancellationToken cancellationToken)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                context.Comments.Add(comment);
                await context.SaveChangesAsync(cancellationToken);

                if (effectiveParentId != null)
                {
                    await context.Database.ExecuteSqlInterpolatedAsync($@"
                        WITH RECURSIVE ancestors AS (
                            SELECT ""Id"", ""ParentCommentId""
                            FROM ""Comments""
                            WHERE ""Id"" = {effectiveParentId}

                            UNION ALL

                            SELECT c.""Id"", c.""ParentCommentId""
                            FROM ""Comments"" c
                            JOIN ancestors a ON c.""Id"" = a.""ParentCommentId""
                        )
                        UPDATE ""Comments""
                        SET ""TotalRepliesCount"" = ""TotalRepliesCount"" + 1
                        WHERE ""Id"" IN (SELECT ""Id"" FROM ancestors)
                    ", cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private sealed class UserInfoProjection
        {
            public string Id { get; init; } = null!;
            public string Username { get; init; } = null!;
            public string UniqueNameIdentifier { get; init; } = null!;
            public bool Blocked { get; init; }
            public string? ImageUrl { get; init; }
            public DateTime LastCommentDate { get; init; }
            public bool PublicationExists { get; init; }
        }
    
    }
}
