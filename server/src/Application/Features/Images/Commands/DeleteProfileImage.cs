using Application.Interfaces.Services;

namespace Application.Features.Images.Commands;

using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteProfileImage
{
    public class Command : IRequest<Result<bool>>
    {
        public required string PublicId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await photoService.DeletePhotoAsync(request.PublicId);
            if (result.Error != null)
                return Result<bool>.Failure($"Image was not deleted due to an error. Error: {result.Error}", 500);

            var image = await context.Images.FirstOrDefaultAsync(a => a.PublicId == request.PublicId, cancellationToken);
            if (image == null) return Result<bool>.Failure($"Image does not exist in the database", 500);
            context.Images.Remove(image);
            return Result<bool>.Success(true);
        }
    }
}
