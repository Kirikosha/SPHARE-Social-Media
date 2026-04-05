using Application.Core;
using Application.Services.ViolationService;
using Domain.DTOs.ViolationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdminFeatures.Commands;

public class DeletePublication
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreateViolationDto Violation { get; set; }
    }

    public class Handler(ApplicationDbContext context, IViolationService violationService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                Publication? publication = await context.Publications.Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == request.Violation.ItemToRemoveId, cancellationToken);

                if (publication == null) return Result<bool>.Failure("Publication to delete was not found", 404);

                publication.IsDeleted = true;
                context.Publications.Update(publication);

                Violation violation = new Violation
                {
                    ViolationText = request.Violation.RemovalReason,
                    ViolatedAt = DateTime.UtcNow,
                    ViolatedBy = publication.Author,
                    ViolatedById = publication.AuthorId
                };

                bool result = await violationService.RegisterViolationAsync(publication.Author, violation, request.Violation.ViolationScoreIncrease, true);

                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Something went wrong during the process. Error: {ex.Message}", 500);
            }
        }
    }
}
