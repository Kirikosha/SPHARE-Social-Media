using Application.Interfaces.Logger;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Features.Publications.Commands;

using Core;
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
            var publication = await context.Publications
                .Select(p => new
                {
                    p.Id,
                    p.AuthorId,
                    ImagePublicIds = p.Images != null ? p.Images.Select(i => i.PublicId).ToList() : null
                }).FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (publication == null)
                return Result<Unit>.Failure("Publication was not deleted because it does not exist", 404);


            await context.Publications
                .Where(p => p.Id == request.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), cancellationToken);
            if (publication.ImagePublicIds is { Count: > 0 })
            {
                await Task.WhenAll(publication.ImagePublicIds
                    .Select(id => photoService.DeletePhotoAsync(id)));
            }
            
            await logger.LogAsync(publication.AuthorId, UserLogAction.DeletePublication, new
            {
                info = $"Publication " +
                       $"was deleted by {publication.AuthorId}"
            }, publication.Id, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
