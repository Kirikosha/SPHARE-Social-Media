namespace Application.Features.Images.Commands;

using Application.Services.PhotoService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteProfileImage
{
    public class Command : IRequest<bool>
    {
        public required string PublicId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await photoService.DeletePhotoAsync(request.PublicId);
            if (result.Error != null) throw new Exception("Image was not deleted");

            var image = await context.Images.FirstOrDefaultAsync(a => a.PublicId == request.PublicId);
            context.Images.Remove(image);
            return true;
        }
    }
}
