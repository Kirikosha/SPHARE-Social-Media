using Application.Core;
using Application.DTOs.TagDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Tags.Commands;

public class AddTag
{
    public class Command : IRequest<Result<Unit>>
    {
        public required CreateTagDto TagDto { get; set; }
        public required string PublicationId { get; set; }
        public required string UserId { get; set; }
    }
    
    public class Handler(ITagService tagService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await tagService.CreateTagAsync(request.UserId, request.PublicationId, request.TagDto,
                cancellationToken);
        }
    }
}