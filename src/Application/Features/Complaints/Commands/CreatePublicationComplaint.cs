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
            var existingComplaint = await context.PublicationComplaints.Where(a =>
                a.PublicationId == request.Complaint.TargetId &&
                a.ComplainerId == request.UserId).FirstOrDefaultAsync(cancellationToken);

            if (existingComplaint != null)
            {
                return Result<bool>.Failure(
                    "Complain on that publication was already sent at " + existingComplaint.ComplainedAt.Date, 400);
            }
            // Check for existing complaint -- End
            
            var res = await spamRepository.MakeComplaint(request.UserId);
            if (!res)
            {
                return Result<bool>.Failure(
                    "You cannot complain for today due to our antispam rules", 400);
            }

            bool isNewAuthor = await IsNewAuthor(request.Complaint.TargetId);
            PublicationComplaint complaint = new PublicationComplaint
            {
                Reason = request.Complaint.Reason,
                Explanation = request.Complaint.Explanation ?? string.Empty,
                ComplainedAt = DateTime.UtcNow,
                ComplainerId = request.UserId,
                PublicationId = request.Complaint.TargetId,
                ComplaintValue = isNewAuthor ?  (BaseComplaintValue * NewAuthorMultiplier) : BaseComplaintValue
            };

            await context.PublicationComplaints.AddAsync(complaint);

            return Result<bool>.Success(true);
        }
        
        private async Task<bool> IsNewAuthor(string publicationId)
        {
            var authorCreationDate = await context.PublicationComplaints
                .Include(a => a.Publication)
                .ThenInclude(c => c.Author)
                .Where(a => a.Publication.Id == publicationId)
                .Select(a => a.Publication.Author.DateOfCreation)
                .FirstAsync();

            var cutOff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            if (authorCreationDate >= cutOff)
            {
                return true;
            }

            return false;
        }
    }

}
