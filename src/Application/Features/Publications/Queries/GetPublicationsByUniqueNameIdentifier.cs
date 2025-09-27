namespace Application.Features.Publications.Queries;

using Application.Core;
using Application.Features.Likes.Queries;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUniqueNameIdentifier
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator) 
        : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.UserId);
            if (user == null)
                return Result<List<PublicationDto>>.Failure("User was not found", 404);

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
                var isLikedResult = await mediator.Send(
                    new IsLikedBy.Query { PublicationId = publication.Id, UserId = request.UserId });

                if (!isLikedResult.IsSuccess)
                {
                    return Result<List<PublicationDto>>.Failure(isLikedResult.Error!, isLikedResult.Code);
                }

                publication.IsLikedByCurrentUser = isLikedResult.Value;
            }

            return Result<List<PublicationDto>>.Success(mappedPublications);
        }
    }
}
