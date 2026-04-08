namespace Application.Features.Publications.Queries;
using Core;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;

public class GetPublicationsCalendar
{
    public class Query : IRequest<Result<List<PublicationCalendarDto>>>
    {
        public required string UserId { get; set; }
    }
    
    public class Handler(IPublicationService publicationService) : IRequestHandler<Query, Result<List<PublicationCalendarDto>>>
    {
        public async Task<Result<List<PublicationCalendarDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await publicationService.GetPublicationsCalendarAsync(request.UserId, cancellationToken);
        }
    }
}