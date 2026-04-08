namespace Application.Features.Publications.Commands;
using DTOs.PublicationDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class UpdatePublication
{
    public class Command : IRequest<Result<PublicationDto>>
    {
        public required UpdatePublicationDto Publication { get; set; }
        public required string UserId { get; set; }
    }

    public class Handler(IPublicationService publicationService) 
        : IRequestHandler<Command, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await publicationService.UpdatePublicationAsync(request.Publication, request.UserId,
                cancellationToken);
        }
    }
}
