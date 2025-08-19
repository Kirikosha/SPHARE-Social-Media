namespace Application.Features.Publications.Queries;

using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUserId
{
    public class Query : IRequest<List<PublicationDto>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Query, List<PublicationDto>>
    {
        public async Task<List<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            List<Publication> publications = await context.Publications
                .Include(a => a.Likes)
                .Include(a => a.Images)
                .Include(a => a.Comments)
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .Where(a => a.AuthorId == request.UserId).ToListAsync(cancellationToken: cancellationToken);
            return mapper.Map<List<PublicationDto>>(publications);
        }
    }
}
