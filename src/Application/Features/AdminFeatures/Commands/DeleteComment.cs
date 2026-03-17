namespace Application.Features.AdminFeatures.Commands;

using Core;
using Services.ViolationService;
using Domain.DTOs.ViolationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreateViolationDto Violation { get; set; }
    }

    public class Handler(ApplicationDbContext context, IViolationService violationService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            try {
                Comment? comment = await context.Comments.Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == request.Violation.ItemToRemoveId, cancellationToken);

                if (comment == null) return Result<bool>.Failure("Comment that was requested to be deleted was not found", 404);

                comment.IsDeleted = true;
                context.Comments.Update(comment);

                Violation violation = new Violation
                {
                    ViolationText = request.Violation.RemovalReason,
                    ViolatedAt = DateTime.UtcNow,
                    ViolatedBy = comment.Author,
                    ViolatedById = comment.AuthorId
                };

                bool result = await violationService.RegisterViolationAsync(comment.Author, violation, request.Violation.ViolationScoreIncrease, false);

                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"During the processing there was exception: {ex.Message}", 500);
            }
        }
    }
}
