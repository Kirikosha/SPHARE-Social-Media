namespace Application.Features.Publications.Queries;

using Application.Features.Likes.Queries;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationById
{
    public class Query : IRequest<PublicationDto>
    {
        public required int PublicationId { get; set; }
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator) 
        : IRequestHandler<Query, PublicationDto>
    {
        public async Task<PublicationDto> Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .Include(a => a.Comments)
                .Include(a => a.Images)
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == request.PublicationId, cancellationToken);

            if (publication == null) throw new Exception("Publication not found");

            var mappedPublication = mapper.Map<PublicationDto>(publication);
            mappedPublication.IsLikedByCurrentUser = await mediator
                .Send(new IsLikedBy.Query { PublicationId = publication.Id, UserId = request.UserId });

            return mappedPublication;
        }
    }
}
