namespace Application.Features.Publications.Commands;

using Application.Services.PhotoService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeletePublication
{
    public class Command : IRequest<Unit>
    {
        public required int Id { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == request.Id);

            if (publication == null) throw new Exception("Not deleted");

            if (publication.Images != null)
            {
                foreach (var image in publication.Images)
                {
                    await photoService.DeletePhotoAsync(image.PublicId!);
                }
            }

            context.Publications.Remove(publication);
            return Unit.Value;
        }
    }
}
