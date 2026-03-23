using Application.Services.UserActionLogger;

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
            Publication? publication = await context.Publications
                .Include(a => a.Images)
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .FirstOrDefaultAsync(a => a.Id == request.Publication.Id, cancellationToken);

            if (publication == null)
                return Result<PublicationDto>.Failure("Publication was not found", 404);

            publication.Content = request.Publication.Content;
            if (publication.PublicationType == PublicationTypes.planned
                && (publication.RemindAt == null || publication.RemindAt != request.Publication.RemindAt)
                && request.Publication.RemindAt > DateTime.UtcNow)
            {
                publication.RemindAt = request.Publication.RemindAt;
            }

            publication.UpdatedAt = DateTime.UtcNow;
            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            var readyPublication = mapper.Map<PublicationDto>(publication);
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

            if (!success) return Result<PublicationDto>.Failure("Publication was not updated", 500);
            await logger.LogAsync(request.UserId, UserLogAction.EditPublication, new
            {
                info = $"User {request
                    .UserId} has updated publication {publication.Id}"
            }, publication.Id, cancellationToken);
            return Result<PublicationDto>.Success(readyPublication);

        }
    }
}
