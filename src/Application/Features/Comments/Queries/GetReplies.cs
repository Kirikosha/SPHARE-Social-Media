using Application.Core;
using Domain.DTOs;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries;
public class GetReplies
{
    public class Query : IRequest<Result<List<CommentDto>>>
    {
        public required string ParentId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<List<CommentDto>>>
    {
        public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool parentExists = await context.Comments
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
                return Result<List<CommentDto>>.Failure("Parent comment does not exist", 404);

            var replies = await context.Comments
                .Where(c => c.ParentCommentId == request.ParentId)
                .OrderBy(c => c.CreationDate)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreationDate = c.CreationDate,
                    PublicationId = c.PublicationId,
                    IsDeleted = c.IsDeleted,

                    Author = new PublicUserDto
                    {
                        Id = c.Author.Id,
                        Blocked = c.Author.Blocked,
                        ProfileImage = c.Author.ProfileImage == null
                            ? null
                            : new ImageDto
                            {
                                Id = c.Author.ProfileImage.Id,
                                PublicId = c.Author.ProfileImage.PublicId!,
                                ImageUrl = c.Author.ProfileImage.ImageUrl
                            },
                        UniqueNameIdentifier = c.Author.UniqueNameIdentifier,
                        Username = c.Author.Username
                    },

                    RepliesAmount = context.CommentTrees
                        .Count(cc => cc.AncestorId == c.Id && cc.Depth > 0)
                })
                .ToListAsync(cancellationToken);

            return Result<List<CommentDto>>.Success(replies);
        }
    }
}
