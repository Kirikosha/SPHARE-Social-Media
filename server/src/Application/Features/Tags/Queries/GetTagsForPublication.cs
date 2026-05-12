using Application.Core;
using Application.DTOs.TagDTOs;
using Application.Interfaces.Services;

namespace Application.Features.Tags.Queries;

public class GetTagsForPublication
{
    public class Query : IRequest<Result<List<TagDto>>>
    {
        public required string PublicationId { get; set; }
    }
    
    public class Handler(ITagService tagService) : IRequestHandler<Query, Result<List<TagDto>>>
    {
        public async Task<Result<List<TagDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await tagService.GetTagsForPublicationAsync(request.PublicationId, cancellationToken);
        }
    }
}