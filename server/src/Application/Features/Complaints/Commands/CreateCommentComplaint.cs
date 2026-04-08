namespace Application.Features.Complaints.Commands;
using Core;
using DTOs.ComplaintDTOs;
using Application.Interfaces.Services;
public class CreateCommentComplaint
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(IComplaintService complaintService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await complaintService.CreateCommentComplaint(request.UserId, request.Complaint, cancellationToken);
        }
    }
}
