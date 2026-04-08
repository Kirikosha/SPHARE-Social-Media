using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.CommentDTOs;

namespace Application.Interfaces.Services;

public interface ICommentService
{
    Task<Result<PagedList<CommentDto>>> GetRepliesAsync(string parentId, int page, int pageSize, CancellationToken ct);

    Task<Result<PagedList<CommentDto>>> GetCommentsByPublicationIdAsync(string publicationId, int page, int pageSize,
        CancellationToken ct);

    Task<Result<int>> GetCommentAmountAsync(string publicationId, CancellationToken ct);
    Task<Result<CommentDto>> GetCommentByIdAsync(string commentId, CancellationToken ct);
    Task<Result<bool>> DeleteCommentAsync(string commentId, CancellationToken ct);
    Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto createDto, CancellationToken ct);
}