namespace Application.Features.Comments.Queries;

using Application.Core;
using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetCommentsByPublicationId
{
    public class Query : IRequest<Result<List<CommentDto>>>
    {
        public required int PublicationId { get; set; }
    }
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<CommentDto>>>
    {
        public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            bool publicationExists = await context.Publications.AnyAsync(a => a.Id == request.PublicationId, cancellationToken);
            if (!publicationExists) return Result<List<CommentDto>>.Failure("Publication does not exist", 404);

            var comments = await context.Comments
                .Where(c => c.PublicationId == request.PublicationId && c.ParentCommentId == null)
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
                        ProfileImage = new ImageDto
                        {
                            Id = c.Author.ProfileImage == null ? -1 : c.Author.ProfileImage.Id,
                            PublicId = c.Author.ProfileImage!.PublicId!,
                            ImageUrl = c.Author.ProfileImage.ImageUrl
                        },
                        UniqueNameIdentifier = c.Author.UniqueNameIdentifier,
                        Username = c.Author.Username
                    },
                    RepliesAmount = context.CommentTrees
                        .Count(cc => cc.AncestorId == c.Id && cc.Depth > 0)
                })
                .ToListAsync(cancellationToken);

            return Result<List<CommentDto>>.Success(mapper.Map<List<CommentDto>>(comments));
        }
    }
}
