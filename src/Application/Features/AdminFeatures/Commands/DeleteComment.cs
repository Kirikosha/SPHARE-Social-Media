namespace Application.Features.AdminFeatures.Commands;

using Application.Services.ViolationService;
using Domain.DTOs.ViolationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<bool>
    {
        public required CreateViolationDto Violation { get; set; }
    }

    public class Handler(ApplicationDbContext context, IViolationService violationService) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            try {
                Comment? comment = await context.Comments.Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == request.Violation.ItemToRemoveId);

                if (comment == null || comment.Author == null) return false;

                string email = comment.Author.Email;
                context.Comments.Remove(comment);

                Violation violation = new Violation
                {
                    ViolationText = request.Violation.RemovalReason,
                    ViolatedAt = DateTime.UtcNow,
                    ViolatedBy = comment.Author,
                    ViolatedById = comment.AuthorId
                };

                bool result = await violationService.RegisterViolationAsync(comment.Author, violation, request.Violation.ViolationScoreIncrease, false);

                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}
