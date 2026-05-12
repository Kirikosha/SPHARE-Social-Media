using Application.Core;
using Application.DTOs.TagDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Tags.Commands;

public class AddTags
{
    public class Command : IRequest<Result<Unit>>
    {
        public required CreateTagsDto TagsDto { get; set; }
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(ITagService tagService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await tagService.CreateTagsAsync(request.UserId, request.PublicationId, request.TagsDto,
                cancellationToken);
        }
    }
}