namespace Application.Features.Publications.Commands;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class CreatePublication
{
    public class Command : IRequest<Result<bool>>
    {
        public required CreatePublicationDto Publication { get; set; }
        public required string CreatorId { get; set; }
    }

    public class Handler(IPublicationService publicationService) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await publicationService.CreatePublicationAsync(request.Publication, request.CreatorId,
                cancellationToken);
        }
    }
}
