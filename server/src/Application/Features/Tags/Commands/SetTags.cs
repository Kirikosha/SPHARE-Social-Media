using Application.Core;
using Application.Interfaces.Services;

namespace Application.Features.Tags.Commands;

public class SetTags
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string UserId { get; set; }
        public required string PublicationId { get; set; }
        public required List<int> TagIds { get; set; }
    }
    
    public class Handler(ITagService tagService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await tagService.SetTagsAsync(request.UserId, request.PublicationId, request.TagIds,
                cancellationToken);
        }
    }
}