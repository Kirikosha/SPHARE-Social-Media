namespace Application.Features.AdminFeatures.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class BlockUser
{
    public class Command : IRequest<Result<bool>>
    {
        public required string UserToBlockId { get; set; }
        public required string BlockedById { get; set; }
    }

    public class Handler(IAdminService adminService) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await adminService.BlockUserAsync(request.UserToBlockId, request.BlockedById, cancellationToken);
        }
    }
}
