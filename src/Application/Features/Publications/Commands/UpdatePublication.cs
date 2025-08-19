namespace Application.Features.Publications.Commands;

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
    public class Command : IRequest<PublicationDto>
    {
        public required UpdatePublicationDto Publication { get; set; }
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator) 
        : IRequestHandler<Command, PublicationDto>
    {
        public async Task<PublicationDto> Handle(Command request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Images)
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .FirstOrDefaultAsync(a => a.Id == request.Publication.Id);

            if (publication == null) throw new Exception("Not found");

            publication.Content = request.Publication.Content;
            if (publication.PublicationType == PublicationTypes.planned
                && (publication.RemindAt == null || publication.RemindAt != request.Publication.RemindAt)
                && request.Publication.RemindAt > DateTime.UtcNow)
            {
                publication.RemindAt = request.Publication.RemindAt;
            }

            publication.UpdatedAt = DateTime.UtcNow;
            bool success = await context.SaveChangesAsync() > 0;
            var readyPublication = mapper.Map<PublicationDto>(publication);
            readyPublication.IsLikedByCurrentUser = await mediator
                .Send(new IsLikedBy.Query { PublicationId = readyPublication.Id, UserId = request.UserId });
            if (success) return readyPublication; 
            throw new Exception("Update was not successful");

        }
    }
}
