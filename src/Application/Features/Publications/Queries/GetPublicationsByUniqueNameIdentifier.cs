namespace Application.Features.Publications.Queries;

using Application.Features.Likes.Queries;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUniqueNameIdentifier
{
    public class Query : IRequest<List<PublicationDto>>
    {
        public required string UniqueNameIdentifier { get; set; }
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator) : IRequestHandler<Query, List<PublicationDto>>
    {
        public async Task<List<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.UserId);
            if (user == null) throw new Exception("User was not found");

            List<Publication> publications = await context.Publications
                .Include(a => a.Likes)
                .Include(a => a.Images)
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .Include(a => a.Comments)
                .Where(a => a.Author.UniqueNameIdentifier == request.UniqueNameIdentifier)
                .ToListAsync();

            var mappedPublications = mapper.Map<List<PublicationDto>>(publications);
            foreach(var publication in mappedPublications)
            {
                publication.IsLikedByCurrentUser = await mediator.Send(
                    new IsLikedBy.Query { PublicationId = publication.Id, UserId = request.UserId });
            }

            return mappedPublications;
        }
    }
}
