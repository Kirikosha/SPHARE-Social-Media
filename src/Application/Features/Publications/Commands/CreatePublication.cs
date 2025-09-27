namespace Application.Features.Publications.Commands;

using Application.Core;
using Application.Features.Images.Commands;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class CreatePublication
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreatePublicationDto Publication { get; set; }
        public required int CreatorId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
        IMediator mediator) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.CreatorId);
            if (user == null)
                return Result<bool>.Failure("Account does not exist therefore publication cannot be created", 403);

            Publication publication = mapper.Map<Publication>(request.Publication);
            publication.Author = user;
            publication.AuthorId = user.Id;

            if (request.Publication.Images != null && request.Publication.Images.Count > 0)
            {
                var images = await mediator.Send(new UploadPublicationImages.Command { Images = request.Publication.Images! });
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
            return Result<bool>.Success(true);
        }
    }
}
