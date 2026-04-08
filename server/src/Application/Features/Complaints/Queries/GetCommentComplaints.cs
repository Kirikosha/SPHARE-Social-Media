namespace Application.Features.Complaints.Queries;
using Core;
using DTOs.ComplaintDTOs;
using Application.Interfaces.Services;
public class GetCommentComplaints
{
    public class Query : IRequest<Result<List<CommentComplaintDto>>> { }
    
    public class Handler(IComplaintService complaintService)
        : IRequestHandler<Query, Result<List<CommentComplaintDto>>>
    {
        public async Task<Result<List<CommentComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await complaintService.GetCommentComplaintsAsync(cancellationToken);
        }
    }
}
