using Application.Core;
using Domain.DTOs.ComplaintDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Features.Complaints.Queries;
public class GetGroupedComplaints
{
    public class Query : IRequest<Result<List<GroupPublicationComplaintDto>>> { }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<List<GroupPublicationComplaintDto>>>
    {
        public async Task<Result<List<GroupPublicationComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await context.PublicationComplaints
                .GroupBy(c => c.PublicationId)
                .Select(g => new GroupPublicationComplaintDto
                {
                    PublicationId = g.Key,
                    TotalComplaints = g.Count(),
                    Reasons = g.GroupBy(c => c.Reason)
                    .Select(r => new ReasonDto
                    {
                        Reason = r.Key,
                        Count = r.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(100)
                    .ToList()
                })
                .OrderByDescending(x => x.TotalComplaints)
                .ToListAsync();

            return Result<List<GroupPublicationComplaintDto>>.Success(result);
        }
    }
}
