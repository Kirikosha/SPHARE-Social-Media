using Application.DTOs.CommentDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommentRepository(ApplicationDbContext context) : ICommentRepository
{
    public async Task<bool> IsCommentExistsByIdAsync(string id, CancellationToken ct)
    {
        return await context.Comments
            .AnyAsync(c => c.Id == id, ct);
    }

    public IQueryable<CommentDto> GetRepliesByParrentCommentId(string parentId)
    {
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

        return query;
    }

    public IQueryable<CommentDto> GetCommentsByPublicationId(string publicationId)
    {
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

        return query;
    }

    public async Task<int> GetCommentCountByPublicationIdAsync(string publicationId, CancellationToken ct)
    {
        return await context.Comments.CountAsync(a => a.PublicationId == publicationId, ct);
    }

    public async Task<CommentDto?> GetCommentByIdAsync(string id, CancellationToken ct)
    {
        var comment = await context.Comments
            .Where(c => c.Id == id)
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

        return comment;
    }

    public async Task<string?> GetCommentAuthorIdByCommentIdAsync(string id, CancellationToken ct)
    {
        var commentData = await context.Comments
            .Where(c => c.Id == id)
            .Select(c => c.AuthorId)
            .FirstOrDefaultAsync(ct);
        return commentData;
    }

    public async Task DeleteCommentAsync(string commentId, CancellationToken ct)
    {
        await context.Comments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true), ct);
    }

    public async Task<string?> GetParentCommentId(string commentId, CancellationToken ct)
    {
        var parentCommentId = await context.Comments
            .Where(c => c.Id == commentId)
            .Select(c => c.ParentCommentId)
            .FirstOrDefaultAsync(ct);
        return parentCommentId;
    }

    public async Task CreateComment(Comment comment, string? effectiveParentId, CancellationToken ct)
    {
        context.Comments.Add(comment);
        await context.SaveChangesAsync(ct);

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
        ", ct);
        }
    }

    public async Task<string?> GetPublicationIdByCommentIdAsync(string commentId, CancellationToken ct)
    {
        var publicationId = await context.Comments
            .Where(c => c.Id == commentId)
            .Select(c => c.PublicationId)
            .FirstOrDefaultAsync(ct);
        return publicationId;
    }
}