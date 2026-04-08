namespace Application.Features.AdminFeatures.Commands;
using DTOs.ViolationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreateViolationDto Violation { get; set; }
    }

    public class Handler(IAdminService adminService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await adminService.DeleteCommentAsync(request.Violation, cancellationToken);
        }
    }
}
