using Application.Core;
using AutoMapper;
using Domain.DTOs.ComplaintDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Complaints.Queries;
public class GetPublicationComplaints
{
    public class Query : IRequest<Result<List<PublicationComplaintDto>>>
    {
    }

    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<List<PublicationComplaintDto>>>
    {
        public async Task<Result<List<PublicationComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publicationComplaints = await context.PublicationComplaints.Include(a => a.Publication).ThenInclude(a => a.Author).ThenInclude(a => a.ProfileImage).ToListAsync();

            return Result<List<PublicationComplaintDto>>
                .Success(mapper.Map<List<PublicationComplaintDto>>(publicationComplaints));
        }
    }
}
