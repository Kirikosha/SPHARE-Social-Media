namespace Application.Features.Publications.Queries;

using Core;
using Application.Features.Likes.Queries;
using AutoMapper;
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

    public class Handler(ApplicationDbContext context, IMapper mapper, IMediator mediator) 
        : IRequestHandler<Query, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .Include(a => a.Comments)
                .Include(a => a.Images)
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == request.PublicationId, cancellationToken);

            if (publication == null)
                return Result<PublicationDto>.Failure("Publication was not found", 404);

            var mappedPublication = mapper.Map<PublicationDto>(publication);
            var isLikedResult = await mediator
                .Send(new IsLikedBy.Query { PublicationId = publication.Id, UserId = request.UserId });
            if (!isLikedResult.IsSuccess)
            {
                return Result<PublicationDto>.Failure(isLikedResult.Error!, isLikedResult.Code);
            }

            mappedPublication.IsLikedByCurrentUser = isLikedResult.Value;
            return Result<PublicationDto>.Success(mappedPublication);
        }
    }
}
