using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.CommentDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CommentService(ApplicationDbContext context, IUserActionLogger<CommentService> logger,
    ISpamRepository spamRepository) : ICommentService
{
    private const int CommentRestrictionInSeconds = 10;
    private const int MaxDepth = 10;
    public async Task<Result<PagedList<CommentDto>>> GetRepliesAsync(string parentId, int page, int pageSize, 
        CancellationToken ct)
    {
        bool parentExists = await context.Comments
            .AnyAsync(c => c.Id == parentId, ct);

        if (!parentExists)
            return Result<PagedList<CommentDto>>.Failure("Parent comment does not exist", 404);

        var query = context.Comments
            .Where(c => c.ParentCommentId == parentId)
            .OrderBy(c => c.CreationDate)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreationDate = c.CreationDate,
                PublicationId = c.PublicationId,
                IsDeleted = c.IsDeleted,
                Author = new PublicUserBriefDto
                {
                    Id = c.Author.Id,
                    Blocked = c.Author.Blocked,
                    ImageUrl = c.Author.ProfileImage == null ? null : c.Author.ProfileImage.ImageUrl,
                    UniqueNameIdentifier = c.Author.UniqueNameIdentifier,
                    Username = c.Author.Username
                },
                RepliesAmount = c.TotalRepliesCount 
            });

        var pagedReplies = await query.ToPagedListAsync(page, pageSize, ct);

        return Result<PagedList<CommentDto>>.Success(pagedReplies);
    }

    public async Task<Result<PagedList<CommentDto>>> GetCommentsByPublicationIdAsync(string publicationId, int page, 
        int pageSize, CancellationToken ct)
    {
        bool publicationExists = await context.Publications
            .AnyAsync(a => a.Id == publicationId, ct);
            
        if (!publicationExists) 
            return Result<PagedList<CommentDto>>.Failure("Publication does not exist", 404);

        var query = context.Comments
            .Where(c => c.PublicationId == publicationId && c.ParentCommentId == null)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreationDate = c.CreationDate,
                PublicationId = c.PublicationId,
                IsDeleted = c.IsDeleted,
                Author = new PublicUserBriefDto
                {
                    Id = c.Author.Id,
                    Blocked = c.Author.Blocked,
                    ImageUrl = c.Author.ProfileImage == null ? null : c.Author.ProfileImage.ImageUrl,
                    UniqueNameIdentifier = c.Author.UniqueNameIdentifier,
                    Username = c.Author.Username
                },
                RepliesAmount = c.TotalRepliesCount
            });

        var pagedComments = await query.ToPagedListAsync(page, pageSize, ct);
        return Result<PagedList<CommentDto>>.Success(pagedComments);
    }

    public async Task<Result<int>> GetCommentAmountAsync(string publicationId, CancellationToken ct)
    {
        var amount = await context.Comments.CountAsync(a => a.PublicationId == publicationId, ct);
        return Result<int>.Success(amount);
    }

    public async Task<Result<CommentDto>> GetCommentByIdAsync(string commentId, CancellationToken ct)
    {
        var comment = await context.Comments
            .Where(c => c.Id == commentId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreationDate = c.CreationDate,
                PublicationId = c.PublicationId,
                IsDeleted = c.IsDeleted,
                Author = new PublicUserBriefDto
                {
                    Id = c.Author.Id,
                    Blocked = c.Author.Blocked,
                    ImageUrl = c.Author.ProfileImage == null ? null : c.Author.ProfileImage.ImageUrl,
                    UniqueNameIdentifier = c.Author.UniqueNameIdentifier,
                    Username = c.Author.Username
                },

                RepliesAmount = c.TotalRepliesCount
            }).SingleOrDefaultAsync(ct);

        return comment == null ? Result<CommentDto>.Failure("Comment was not found", 404) 
            : Result<CommentDto>.Success(comment);
    }

    public async Task<Result<bool>> DeleteCommentAsync(string commentId, CancellationToken ct)
    {
        var comment = await context.Comments
            .Where(c => c.Id == commentId)
            .Select(c => new { c.Id, c.AuthorId })
            .FirstOrDefaultAsync(ct);

        if (comment == null) 
            return Result<bool>.Failure("Comment was not found", 404);

        await context.Comments
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true), ct);

        await logger.LogAsync(comment.AuthorId, UserLogAction.DeleteComment, new
        {
            info = $"Comment {comment.Id} was " +
                   $"deleted by user {comment.AuthorId}"
        }, comment.Id, ct);
            
        return Result<bool>.Success(true);
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto createDto, 
        CancellationToken ct)
    {
        var userInfo = await GetUserInfoAsync(userId, createDto.PublicationId, ct);

        if (userInfo == null)
            return Result<CommentDto>.Failure("User was not found", 400);

        if (!userInfo.PublicationExists)
            return Result<CommentDto>.Failure("Publication you are trying to comment on no longer exists", 400);

        var cutOff = DateTime.UtcNow.AddSeconds(-CommentRestrictionInSeconds);
        if (userInfo.LastCommentDate > cutOff)
            return Result<CommentDto>.Failure("You are sending comments too fast", 400);

        var spamResult = await spamRepository.MakeComment(userId, ct);
        if (!spamResult)
            return Result<CommentDto>.Failure("You cannot make comments for today due to our antispam rules", 400);

        var parentValidation = await ValidateParentAsync(createDto.ParentCommentId, createDto.PublicationId, ct);
        if (!parentValidation.IsSuccess)
            return Result<CommentDto>.Failure(parentValidation.Error!, parentValidation.Code);

        var effectiveParentId = await ResolveEffectiveParentIdAsync(
            createDto.ParentCommentId, ct);

        var comment = new Comment
        {
            AuthorId = userId,
            PublicationId = createDto.PublicationId,
            Content = createDto.Content,
            CreationDate = DateTime.UtcNow,
            ParentCommentId = effectiveParentId
        };

        await PersistCommentAsync(comment, effectiveParentId, ct);

        await logger.LogAsync(userId, UserLogAction.CreateComment, new
        {
            info = $"Comment {comment.Id} was created by user {userId}"
        }, comment.PublicationId, ct);

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
        string userId, string publicationId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.Id == userId)
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
                    .Any(p => p.Id == publicationId)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Result<bool>> ValidateParentAsync(
        string? parrentCommentId, string publicationId, CancellationToken cancellationToken)
    {
        if (parrentCommentId == null)
            return Result<bool>.Success(true);

        var snapshot = parrentCommentId;
        var parentInfo = await context.Comments
            .Where(c => c.Id == snapshot)
            .Select(c => new { c.PublicationId })
            .FirstOrDefaultAsync(cancellationToken);

        if (parentInfo == null)
            return Result<bool>.Failure("Parent comment you are responding to no longer exists", 400);

        if (parentInfo.PublicationId != publicationId)
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