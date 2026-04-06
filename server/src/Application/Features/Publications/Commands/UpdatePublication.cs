using Application.Interfaces.Logger;
using Domain.DTOs.UserDTOs;

namespace Application.Features.Publications.Commands;

using Core;
using Application.Features.Likes.Queries;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class UpdatePublication
{
    public class Command : IRequest<Result<PublicationDto>>
    {
        public required UpdatePublicationDto Publication { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator, 
        IUserActionLogger<UpdatePublication> logger) 
        : IRequestHandler<Command, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var publication = await context.Publications
                .Where(p => p.Id == request.Publication.Id)
                .Select(p => new
                {
                    p.Id,
                    p.AuthorId,
                    p.PublicationType,
                    p.RemindAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (publication == null)
                return Result<PublicationDto>.Failure("Publication was not found", 404);

            await context.Publications
                .Where(p => p.Id == request.Publication.Id)
                .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Content, request.Publication.Content)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            if (publication.PublicationType == PublicationTypes.planned
                && request.Publication.RemindAt > DateTime.UtcNow
                && publication.RemindAt != request.Publication.RemindAt)
            {
                await context.Publications
                    .Where(p => p.Id == request.Publication.Id)
                    .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.RemindAt, request.Publication.RemindAt),
                        cancellationToken);
            }
            
            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success) return Result<PublicationDto>.Failure("Publication was not updated", 500);
            
            var readyPublication = await context.Publications
                .Where(p => p.Id == request.Publication.Id)
                .Select(p => new PublicationDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,
                    RemindAt = p.RemindAt,
                    LikesAmount = p.Likes.Count,
                    CommentAmount = p.Comments == null ? 0 : p.Comments.Count,
                    PublicationType = p.PublicationType,
                    ConditionType = p.ConditionType,
                    ConditionTarget = p.ConditionTarget,
                    Author = new PublicUserBriefDto
                    {
                        Id = p.Author.Id,
                        Username = p.Author.Username,
                        UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                        Blocked = p.Author.Blocked,
                        ImageUrl = p.Author.ProfileImage == null ? null : p.Author.ProfileImage.ImageUrl
                    },
                    ComparisonOperator = p.ComparisonOperator,
                    ViewCount = p.ViewCount,
                    IsDeleted = p.IsDeleted,
                    Images = p.Images!
                        .Select(x => x.ImageUrl)
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            var isLikedResult = await mediator
                .Send(new IsLikedBy.Query { PublicationId = readyPublication.Id, UserId = request.UserId }, cancellationToken);
            if (isLikedResult.IsSuccess)
            {
                readyPublication.IsLikedByCurrentUser = isLikedResult.Value;
            }
            else
            {
                return Result<PublicationDto>.Failure(isLikedResult.Error!, isLikedResult.Code);
            }


            await logger.LogAsync(request.UserId, UserLogAction.EditPublication, new
            {
                info = $"User {request
                    .UserId} has updated publication {publication.Id}"
            }, publication.Id, cancellationToken);
            return Result<PublicationDto>.Success(readyPublication);

        }
    }
}
