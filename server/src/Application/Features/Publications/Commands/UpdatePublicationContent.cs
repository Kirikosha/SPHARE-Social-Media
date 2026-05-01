using Application.Core;
using Application.DTOs.PublicationDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Publications.Commands;

public class UpdatePublicationContent
{
    public class Command : IRequest<Result<PublicationDto>>
    {
        public required UpdatePublicationContentDto UpdateContentDto { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(IPublicationService publicationService)
        : IRequestHandler<Command, Result<PublicationDto>>
    {
        public async Task<Result<PublicationDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await publicationService.UpdatePublicationContentAsync(request.UpdateContentDto, request.UserId,
                cancellationToken);
        }
    }
}