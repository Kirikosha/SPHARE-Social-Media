namespace Application.Features.Publications.Commands;

using Application.Core;
using Application.Services.PhotoService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeletePublication
{
    public class Command : IRequest<Result<Unit>>
    {
        public required int Id { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == request.Id);

            if (publication == null)
                return Result<Unit>.Failure("Publication was not deleted because it does not exist", 404);

            if (publication.Images != null)
            {
                foreach (var image in publication.Images)
                {
                    await photoService.DeletePhotoAsync(image.PublicId!);
                }
            }

            context.Publications.Remove(publication);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
