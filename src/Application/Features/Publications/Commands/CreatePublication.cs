namespace Application.Features.Publications.Commands;

using Application.Features.Images.Commands;
using AutoMapper;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class CreatePublication
{
    public class Command : IRequest<bool>
    {
        public required CreatePublicationDto Publication { get; set; }
        public required int CreatorId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper,
        IMediator mediator) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FindAsync(request.CreatorId);
            if (user == null) throw new Exception("User does not exist");

            Publication publication = mapper.Map<Publication>(request.Publication);
            publication.Author = user;
            publication.AuthorId = user.Id;

            if (request.Publication.Images != null && request.Publication.Images.Count > 0)
            {
                var images = await mediator.Send(new UploadPublicationImages.Command { Images = request.Publication.Images! });

                publication.Images = images;
            }

            context.Add(publication);
            return true;
        }
    }
}
