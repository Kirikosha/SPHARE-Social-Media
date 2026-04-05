using Application.Core;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Publications.Queries;

public class GetPlannedPublications
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required string UserId { get; set; }
    }
    
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publications = await context.Publications
                .Where(a => a.RemindAt != null && a.AuthorId == request.UserId)
                .ToListAsync(cancellationToken);

            var mapped = mapper.Map<List<PublicationDto>>(publications);
            return Result<List<PublicationDto>>.Success(mapped);
        }
    }
}