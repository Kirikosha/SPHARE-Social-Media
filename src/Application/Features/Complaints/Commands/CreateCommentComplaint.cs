using Application.Core;
using Domain.DTOs.ComplaintDTOs;
using Domain.Entities.Complaints;
using Infrastructure;

namespace Application.Features.Complaints.Commands;
public class CreateCommentComplaint
{
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            CommentComplaint complaint = new CommentComplaint
            {
                Reason = request.Complaint.Reason,
                Explanation = request.Complaint.Explanation ?? string.Empty,
                ComplainedAt = DateTime.UtcNow,
                ComplainerId = request.UserId,
                CommentId = request.Complaint.TargetId
            };

            await context.CommentComplaints.AddAsync(complaint, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
