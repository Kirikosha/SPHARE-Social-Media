namespace Application.Features.Complaints.Queries;
using Core;
using DTOs.ComplaintDTOs;
using Application.Interfaces.Services;
public class GetGroupedComplaints
{
    public class Query : IRequest<Result<List<GroupPublicationComplaintDto>>> { }
    public class Handler(IComplaintService complaintService) : IRequestHandler<Query, Result<List<GroupPublicationComplaintDto>>>
    {
        public async Task<Result<List<GroupPublicationComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await complaintService.GetGroupedComplaintsAsync(cancellationToken);
        }
    }
}
