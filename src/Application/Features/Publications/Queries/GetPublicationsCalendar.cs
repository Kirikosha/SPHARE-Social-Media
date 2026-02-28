using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTOs.PublicationDTOs;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Publications.Queries;

public class GetPublicationsCalendar
{
    public class Query : IRequest<Result<List<PublicationCalendarDto>>>
    {
        public required int UserId { get; set; }
    }
    
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<PublicationCalendarDto>>>
    {
        public async Task<Result<List<PublicationCalendarDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publications = await context.Publications
                .Where(a => a.AuthorId == request.UserId && a.PublicationType == PublicationTypes.planned)
                .ProjectTo<PublicationCalendarDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<PublicationCalendarDto>>.Success(publications);
        }
    }
}