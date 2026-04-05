using Application.Core;
using Application.Repositories.SpamRepository;
using Domain.DTOs.ComplaintDTOs;
using Domain.Entities.Complaints;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Complaints.Commands;
public class CreatePublicationComplaint
{
    private const double BaseComplaintValue = 1.0;
    private const double NewAuthorMultiplier = 2.0;
    //private const double ComplaintLimit = 20.0;
    // Изменить возвращаемый тип на дтошку
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(ApplicationDbContext context, ISpamRepository spamRepository) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check for existing complaint -- Start
            var existingComplaint = await context.PublicationComplaints
                .Where(a => a.PublicationId == request.Complaint.TargetId &&
                            a.ComplainerId == request.UserId)
                .Select(c => new { c.ComplainedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (existingComplaint != null)
            {
                return Result<bool>.Failure(
                    "Complain on that publication was already sent at " + existingComplaint.ComplainedAt.Date, 400);
            }
            
            var res = await spamRepository.MakeComplaint(request.UserId, cancellationToken);
            if (!res)
            {
                return Result<bool>.Failure(
                    "You cannot complain for today due to our antispam rules", 400);
            }

            bool isNewAuthor = await IsNewAuthor(request.Complaint.TargetId, cancellationToken);
            PublicationComplaint complaint = new PublicationComplaint
            {
                Reason = request.Complaint.Reason,
                Explanation = request.Complaint.Explanation ?? string.Empty,
                ComplainedAt = DateTime.UtcNow,
                ComplainerId = request.UserId,
                PublicationId = request.Complaint.TargetId,
                ComplaintValue = isNewAuthor ?  (BaseComplaintValue * NewAuthorMultiplier) : BaseComplaintValue
            };

            await context.PublicationComplaints.AddAsync(complaint, cancellationToken);

            return Result<bool>.Success(true);
        }
        
        private async Task<bool> IsNewAuthor(string publicationId, CancellationToken ct)
        {
            var authorCreationDate = await context.Publications
                .Where(p => p.Id == publicationId)
                .Select(p => p.Author.DateOfCreation)
                .FirstOrDefaultAsync(ct);

            if (authorCreationDate == default)
                throw new Exception("Author cannot have default creation date");

            var cutOff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            return authorCreationDate >= cutOff;
        }
    }
}
