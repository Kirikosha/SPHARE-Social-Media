namespace Application.Features.Publications.Commands;
using Core;
using Application.Interfaces.Services;
public class UpdatePublicationViews
{
    public class Command : IRequest<Result<int>>
    {
        public required string PublicationId { get; set; }
    }
    
    public class Handler(IPublicationService publicationService) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await publicationService.UpdatePublicationViewsAsync(request.PublicationId, cancellationToken);
        }
    }
}