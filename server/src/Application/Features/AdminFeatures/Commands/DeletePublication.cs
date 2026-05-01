namespace Application.Features.AdminFeatures.Commands;
using Core;
using DTOs.ViolationDTOs;
using Application.Interfaces.Services;

public class DeletePublication
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreateViolationDto Violation { get; set; }
        public required string AdminId { get; set; }
    }

    public class Handler(IAdminService adminService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await adminService.DeletePublicationAsync(request.Violation, request.AdminId, cancellationToken);
        }
    }
}
