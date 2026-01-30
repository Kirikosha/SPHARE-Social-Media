using Application.Core;
using Infrastructure;

namespace Application.Features.Complaints.Commands;
public class DeleteComplaint
{
    public class Command : IRequest<Result<bool>>
    {
        public int TargetId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var complaint = await context.Complaints.FindAsync(request.TargetId);
            if (complaint == null)
            {
                return Result<bool>.Failure("Complaint was not found", 404);
            }

            context.Complaints.Remove(complaint);

            return Result<bool>.Success(true);
        }
    }
}
