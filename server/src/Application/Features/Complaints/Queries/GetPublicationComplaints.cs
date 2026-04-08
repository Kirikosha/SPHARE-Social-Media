namespace Application.Features.Complaints.Queries;
using Core;
using DTOs.ComplaintDTOs;
using Application.Interfaces.Services;
public class GetPublicationComplaints
{
    public class Query : IRequest<Result<List<PublicationComplaintDto>>>
    {
    }

    public class Handler(IComplaintService complaintService)
        : IRequestHandler<Query, Result<List<PublicationComplaintDto>>>
    {
        public async Task<Result<List<PublicationComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await complaintService.GetPublicationComplaintsAsync(cancellationToken);
        }
    }
}
