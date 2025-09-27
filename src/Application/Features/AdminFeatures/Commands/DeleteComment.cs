namespace Application.Features.AdminFeatures.Commands;

using Application.Core;
using Application.Services.ViolationService;
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
                    .FirstOrDefaultAsync(a => a.Id == request.Violation.ItemToRemoveId);

                if (comment == null || comment.Author == null) return Result<bool>.Failure("Comment that was requested to be deleted was not found", 404);

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

                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"During the processing there was exception: {ex.Message}", 500);
            }
        }
    }
}
