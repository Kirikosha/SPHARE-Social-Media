using Domain.DTOs.UserDTOs;

namespace Application.Features.Publications.Queries;

using Core;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationById
{
    public class Query : IRequest<Result<PublicationDto>>
    {
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context) 
        : IRequestHandler<Query, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publication = await context.Publications
                .Where(p => p.Id == request.PublicationId)
                .Select(p => new PublicationDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,
                    RemindAt = p.RemindAt,
                    PublicationType = p.PublicationType,
                    ConditionType = p.ConditionType,
                    ConditionTarget = p.ConditionTarget,
                    ComparisonOperator = p.ComparisonOperator,
                    ViewCount = p.ViewCount,
                    IsDeleted = p.IsDeleted,
                    LikesAmount = p.Likes.Count,
                    CommentAmount = p.Comments!.Count(c => c.ParentCommentId == null),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == request.UserId),
                    Images = p.Images!.Select(i => i.ImageUrl).ToList(),
                    Author = new PublicUserBriefDto
                    {
                        Id = p.Author.Id,
                        Username = p.Author.Username,
                        UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                        Blocked = p.Author.Blocked,
                        ImageUrl = p.Author.ProfileImage == null ? null : p.Author.ProfileImage.ImageUrl
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (publication == null)
                return Result<PublicationDto>.Failure("Publication was not found", 404);

            return Result<PublicationDto>.Success(publication);
        }
    }
}
