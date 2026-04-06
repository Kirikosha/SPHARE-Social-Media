using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Publications.Commands;

using Core;
using Application.Features.Images.Commands;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class CreatePublication
{
    // normal user
    //private const int PublicationNumberLimit = 3;
    private const int PublicationTimeLimit = 5; // in minutes
    private const int PublicationNumberLimit = 50;
    
    // new user
    private const int NewUserPublicationNumberLimit = 1;
    private const int NewUserPublicationTimeLimit = 10; // in minutes
    public class Command : IRequest<Result<bool>>
    {
        public required CreatePublicationDto Publication { get; set; }
        public required string CreatorId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
        IMediator mediator, ISpamRepository spamRepository, IUserActionLogger<CreatePublication> logger) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.CreatorId, cancellationToken);
            if (user == null)
                return Result<bool>
                    .Failure("Account does not exist therefore publication cannot be created", 403);

            bool isPublicationSpamming = await IsPublicationSpamming(user.Id, user.DateOfCreation, cancellationToken);
            if (isPublicationSpamming)
                return Result<bool>
                    .Failure("You cannot make that many publications in short period of time", 400);

            var res = await spamRepository.MakePublication(user.Id, cancellationToken);
            if (!res)
            {
                return Result<bool>.Failure(
                    "You cannot make more publications for today due to our anti spam rules", 400);
            }
            
            Publication publication = mapper.Map<Publication>(request.Publication);
            publication.Author = user;
            publication.AuthorId = user.Id;

            if (request.Publication.Images != null && request.Publication.Images.Count > 0)
            {
                var images = await mediator.Send(new UploadPublicationImages.Command { Images = request.Publication
                    .Images! }, cancellationToken);
                if (images.IsSuccess)
                {
                    publication.Images = images.Value;
                }
                else
                {
                    return Result<bool>.Failure(images.Error!, images.Code);
                }

            }

            context.Add(publication);

            await logger.LogAsync(request.CreatorId, UserLogAction.CreatePublication, new
            {
                info = $"User {request
                    .CreatorId} has created a publication {publication.Id}"
            }, publication.Id, cancellationToken);
            
            return Result<bool>.Success(true);
        }

        private async Task<bool> IsPublicationSpamming(string userId, DateOnly userCreationDate, CancellationToken ct)
        {
            bool isUserNew = userCreationDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            
            // Set limits based on user status
            int timeLimitMinutes = isUserNew ? NewUserPublicationTimeLimit : PublicationTimeLimit;
            int? maxPublications = isUserNew ? NewUserPublicationNumberLimit : PublicationNumberLimit;

            // BUG FIX: Subtract minutes to look into the past, not the future
            var cutOffTime = DateTime.UtcNow.AddMinutes(-timeLimitMinutes);

            // Run a single, clean query
            int recentPublicationCount = await context.Publications
                .CountAsync(a => a.AuthorId == userId && a.PostedAt >= cutOffTime, ct);

            return recentPublicationCount >= maxPublications;
        }
    }
}
