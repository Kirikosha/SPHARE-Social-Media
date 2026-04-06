using Application.Core;
using Application.Interfaces.Repositories;
using Domain.DTOs.ComplaintDTOs;
using Domain.Entities.Complaints;
using Infrastructure;

namespace Application.Features.Complaints.Commands;
public class CreateCommentComplaint
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISpamRepository spamRepository) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var res = await spamRepository.MakeComplaint(request.UserId, cancellationToken);
            if (!res)
            {
                return Result<bool>.Failure(
                    "You cannot complain for today due to our antispam rules", 400);
            }
            
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
