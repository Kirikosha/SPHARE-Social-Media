namespace Application.Features.AdminFeatures.Commands;
using Application.Interfaces.Services;
using Core;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UnblockUser
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
    }

    public class Handler(IAdminService adminService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await adminService.UnblockUserAsync(request.UserId, cancellationToken);
        }
    }
}
