using Application.Core;
using Application.Core.Pagination;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries;
public class GetReplies
{
    public class Query : IRequest<Result<PagedList<CommentDto>>>
    {
        public required string ParentId { get; set; }
        public PaginationParams Params { get; init; } = new();
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<PagedList<CommentDto>>>
    {
        public async Task<Result<PagedList<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool parentExists = await context.Comments
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
                return Result<PagedList<CommentDto>>.Failure("Parent comment does not exist", 404);

            var query = context.Comments                           // ← no ToListAsync, keep IQueryable
                .Where(c => c.ParentCommentId == request.ParentId)
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

            var pagedReplies = await PagedList<CommentDto>.CreateAsync(
                query, request.Params.PageNumber, request.Params.PageSize, cancellationToken);

            return Result<PagedList<CommentDto>>.Success(pagedReplies);
        }
    }
}