using Application.Core;
using Application.DTOs.TagDTOs;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.RecomendationEntities;

namespace Infrastructure.Services;

public class PublicationTagService(IPublicationRepository publicationRepository, IMlTaggingService mlTaggingService,
    IMapper mapper, ITagRepository tagRepository) : IPublicationTagService
{
    public async Task<Result<List<GeneratedTagDto>>> GenerateTags(string publicationId, string userId, CancellationToken
            ct)
    {
        if (!await publicationRepository.IsUserAuthorAsync(userId, publicationId, ct))
            return Result<List<GeneratedTagDto>>.Failure(PublicationErrors.NotAuthorised());
        var publication = await publicationRepository.GetRawPublicationByIdAsync(publicationId, ct);
        if (publication == null)
            return Result<List<GeneratedTagDto>>.Failure(PublicationErrors.NotFound());
        if (string.IsNullOrEmpty(publication.Content))
            return Result<List<GeneratedTagDto>>.Failure("Publication must have at least some text for the tags to be generated",
                400);

        var userManualTags = await tagRepository.GetTagsForPublicationAsync(publicationId, ct);
        
        var extractionResult = await mlTaggingService.ExtractTagsAsync(publication.Content, userManualTags.Select(x 
            => x.Name).ToList(), ct);
        if (extractionResult == null)
            return Result<List<GeneratedTagDto>>.Failure("ML service returned no result", 500);

        List<Tag> tags = new List<Tag>();
        foreach (var tagName in extractionResult.NewTags)
        {
            tags.Add(new Tag() {Name = tagName});
        }

        return mapper.Map<List<GeneratedTagDto>>(tags);
    }
}