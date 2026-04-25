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

public class CommentService(IUserActionLogger<CommentService> logger,
    ISpamRepository spamRepository, ICommentRepository commentRepository, IPublicationRepository publicationRepository
    , IUserRepository userRepository) : ICommentService
{
    private const int CommentRestrictionInSeconds = 10;
    private const int MaxDepth = 10;
    public async Task<Result<PagedList<CommentDto>>> GetRepliesAsync(string parentId, int page, int pageSize, 
        CancellationToken ct)
    {
        bool parentExists = await commentRepository.IsCommentExistsByIdAsync(parentId, ct);

        if (!parentExists)
            return Result<PagedList<CommentDto>>.Failure("Parent comment does not exist", 404);

        var query = commentRepository.GetRepliesByParrentCommentId(parentId);

        var pagedReplies = await query.ToPagedListAsync(page, pageSize, ct);

        return Result<PagedList<CommentDto>>.Success(pagedReplies);
    }

    public async Task<Result<PagedList<CommentDto>>> GetCommentsByPublicationIdAsync(string publicationId, int page, 
        int pageSize, CancellationToken ct)
    {
        bool publicationExists = await publicationRepository.IsPublicationExistsAsync(publicationId, ct);
            
        if (!publicationExists) 
            return Result<PagedList<CommentDto>>.Failure("Publication does not exist", 404);

        var query = commentRepository.GetCommentsByPublicationId(publicationId);

        var pagedComments = await query.ToPagedListAsync(page, pageSize, ct);
        return Result<PagedList<CommentDto>>.Success(pagedComments);
    }

    public async Task<Result<int>> GetCommentAmountAsync(string publicationId, CancellationToken ct)
    {
        var amount = await commentRepository.GetCommentCountByPublicationIdAsync(publicationId, ct);
        return Result<int>.Success(amount);
    }

    public async Task<Result<CommentDto>> GetCommentByIdAsync(string commentId, CancellationToken ct)
    {
        var comment = await commentRepository.GetCommentByIdAsync(commentId, ct);
        return comment == null ? Result<CommentDto>.Failure("Comment was not found", 404) 
            : Result<CommentDto>.Success(comment);
    }

    public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId, bool isUserAdmin, 
        CancellationToken ct)
    {
        var commentAuthorId = await commentRepository.GetCommentAuthorIdByCommentIdAsync(commentId, ct);

        if (string.IsNullOrEmpty(commentAuthorId)) 
            return Result<bool>.Failure("Comment was not found", 404);

        if (!isUserAdmin && userId != commentAuthorId)
            return Result<bool>.Failure("Comment does not belong to you", 401);

        await commentRepository.DeleteCommentAsync(commentId, ct);

        await logger.LogAsync(commentAuthorId, UserLogAction.DeleteComment, new
        {
            info = $"Comment {commentId} was " +
                   $"deleted by user {commentAuthorId}"
        }, commentId, ct);
            
        return Result<bool>.Success(true);
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto createDto, 
        CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoAsync(userId, createDto.PublicationId, ct);

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

        await commentRepository.CreateComment(comment, effectiveParentId, ct);

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
    


    private async Task<Result<bool>> ValidateParentAsync(
        string? parrentCommentId, string publicationId, CancellationToken ct)
    {
        if (parrentCommentId == null)
            return Result<bool>.Success(true);

        var snapshot = parrentCommentId;
        var parentInfo = await commentRepository.GetPublicationIdByCommentIdAsync(snapshot, ct);

        if (string.IsNullOrEmpty(parentInfo))
            return Result<bool>.Failure("Parent comment you are responding to no longer exists", 400);

        if (parentInfo != publicationId)
            return Result<bool>.Failure("Comment reply internal server error. Try again later!", 500);

        return Result<bool>.Success(true);
    }

    private async Task<string?> ResolveEffectiveParentIdAsync(
        string? parentId, CancellationToken ct)
    {
        if (parentId == null) return null;

        int depth = 0;
        string? currentId = parentId;

        while (currentId != null)
        {
            var snapshot = currentId;
            var parent = await commentRepository.GetParentCommentId(snapshot, ct);

            if (parent == null) break;
            depth++;
            currentId = parent;
        }

        if (depth + 1 <= MaxDepth)
            return parentId;

        int stepsUp = depth - (MaxDepth - 1);
        currentId = parentId;

        for (int i = 0; i < stepsUp; i++)
        {
            var snapshot = currentId;
            currentId = await commentRepository.GetParentCommentId(snapshot!, ct);

        }
        return currentId;
    }
}