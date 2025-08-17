namespace Application.Features.AdminFeatures.Commands;

using Application.Services.ViolationService;
using Domain.DTOs.ViolationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeletePublication
{
    public class Command : IRequest<bool>
    {
        public required CreateViolationDto Violation { get; set; }
    }

    public class Handler(ApplicationDbContext context, IViolationService violationService) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                Publication? publication = await context.Publications.Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == request.Violation.ItemToRemoveId);

                if (publication == null || publication.Author == null) return false;

                string email = publication.Author.Email;
                context.Publications.Remove(publication);

                Violation violation = new Violation
                {
                    ViolationText = request.Violation.RemovalReason,
                    ViolatedAt = DateTime.UtcNow,
                    ViolatedBy = publication.Author,
                    ViolatedById = publication.AuthorId
                };

                bool result = await violationService.RegisterViolationAsync(publication.Author, violation, request.Violation.ViolationScoreIncrease, true);

                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}
