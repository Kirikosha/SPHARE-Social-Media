using Application.Core.Pagination;

namespace Application.Features.Comments.Queries;

using Core;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetCommentsByPublicationId
{
    public class Query : IRequest<Result<PagedList<CommentDto>>>
    {
        public required string PublicationId { get; init; }
        public PaginationParams Params { get; init; } = new();
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<PagedList<CommentDto>>>
    {
        public async Task<Result<PagedList<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool publicationExists = await context.Publications
                .AnyAsync(a => a.Id == request.PublicationId, cancellationToken);
            
            if (!publicationExists) 
                return Result<PagedList<CommentDto>>.Failure("Publication does not exist", 404);

            var query = context.Comments
                .Where(c => c.PublicationId == request.PublicationId && c.ParentCommentId == null)
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
                    RepliesAmount = context.CommentTrees
                        .Count(cc => cc.AncestorId == c.Id && cc.Depth > 0)
                });

            var pagedComments = await PagedList<CommentDto>.CreateAsync(
                query, request.Params.PageNumber, request.Params.PageSize, cancellationToken);

            return Result<PagedList<CommentDto>>.Success(pagedComments);
        }
    }
}
