using Application.DTOs.CommentDTOs;

namespace Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<bool> IsCommentExistsByIdAsync(string id, CancellationToken ct);
    IQueryable<CommentDto> GetRepliesByParrentCommentId(string parrentId);
    IQueryable<CommentDto> GetCommentsByPublicationId(string publicationId);
    Task<int> GetCommentCountByPublicationIdAsync(string publicationId, CancellationToken ct);
    Task<CommentDto?> GetCommentByIdAsync(string id, CancellationToken ct);
    Task<string?> GetCommentAuthorIdByCommentIdAsync(string id, CancellationToken ct);
    Task DeleteCommentAsync(string commentId, CancellationToken ct);
    Task<string?> GetParentCommentId(string commentId, CancellationToken ct);
    Task CreateComment(Comment comment, string? effectiveParentId, CancellationToken ct);
    Task<string?> GetPublicationIdByCommentIdAsync(string commentId, CancellationToken ct);
}