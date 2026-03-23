using Application.Services.UserActionLogger;
using Domain.Enums;

namespace Application.Features.Publications.Commands;

using Core;
using Services.PhotoService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeletePublication
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
    }

    public class Handler(ApplicationDbContext context, IPhotoService photoService, 
        IUserActionLogger<DeletePublication> logger) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications
                .Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (publication == null)
                return Result<Unit>.Failure("Publication was not deleted because it does not exist", 404);

            if (publication.Images != null)
            {
                foreach (var image in publication.Images)
                {
                    await photoService.DeletePhotoAsync(image.PublicId!);
                }
            }

            publication.IsDeleted = true;
            context.Publications.Update(publication);

            await logger.LogAsync(publication.AuthorId, UserLogAction.DeletePublication, new
            {
                info = $"Publication " +
                       $"was deleted by {publication.AuthorId}"
            }, publication.Id, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
