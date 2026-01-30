using Application.Core;
using Domain.DTOs;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries;
public class GetComment
{
    public class Query : IRequest<Result<CommentDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var comment = await context.Comments
                .Where(c => c.Id == request.Id)
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
                }).SingleOrDefaultAsync();

            if (comment == null) return Result<CommentDto>.Failure("Comment was not found", 404);
            return Result<CommentDto>.Success(comment);
        }
    }
}
