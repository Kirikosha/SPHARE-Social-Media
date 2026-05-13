using Application.Core;
using Application.DTOs.TagDTOs;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.RecomendationEntities;
using MediatR;

namespace Infrastructure.Services;

public class TagService(ITagRepository tagRepository, IPublicationRepository publicationRepository,
    IMapper mapper) : ITagService
{
    public async Task<Result<Unit>> CreateTagAsync(string userId, string publicationId, CreateTagDto tagDto, 
        CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;
        
        var amountValidationResult = await VerifyTagLimit(1, publicationId, ct);
        if (!amountValidationResult.IsSuccess)
            return amountValidationResult;

        var tag = new Tag
        {
            Name = tagDto.Name.Trim().ToLowerInvariant(),
        };

        var tagId = await tagRepository.AddTagAsync(tag, ct);

        await tagRepository.SetTagForPublicationAsync(publicationId, tag, ct);

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> CreateTagsAsync(string userId, string publicationId, CreateTagsDto tagsDto, 
        CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;
        
        var amountValidationResult = await VerifyTagLimit(tagsDto.Tags.Count, publicationId, ct);
        if (!amountValidationResult.IsSuccess)
            return amountValidationResult;

        var tags = tagsDto.Tags.Select(tag => new Tag
        {
            Name = tag.Name.Trim().ToLowerInvariant()
        }).ToList();

        var tagIds = await tagRepository.AddTagsAsync(tags, ct);

        await tagRepository.SetTagsForPublicationAsync(publicationId, tagIds, ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> SetTagAsync(string userId, string publicationId, int tagId, CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;
        
        var amountValidationResult = await VerifyTagLimit(tagId, publicationId, ct);
        if (!amountValidationResult.IsSuccess)
            return amountValidationResult;
        

        await tagRepository.SetTagForPublicationAsync(publicationId, tagId, ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> SetTagsAsync(string userId, string publicationId, List<int> tagsId, 
        CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;

        var amountValidationResult = await VerifyTagLimit(tagsId.Count, publicationId, ct);
        if (!amountValidationResult.IsSuccess)
            return amountValidationResult;

        await tagRepository.SetTagsForPublicationAsync(publicationId, tagsId, ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<TagDto>>> SearchTags(string searchName, CancellationToken ct)
    {
        var tags = await tagRepository.SearchTagsByNameAsync(searchName.Trim().ToLowerInvariant(), ct);
        return mapper.Map<List<TagDto>>(tags);
    }

    public async Task<Result<List<TagDto>>> GetTagsForPublicationAsync(string publicationId, CancellationToken ct)
    {
        var tags = await tagRepository.GetTagsForPublicationAsync(publicationId, ct);
        return mapper.Map<List<TagDto>>(tags);
    }

    public async Task<Result<Unit>> DeleteTagPublicationAsync(string userId, string publicationId, int tagId, 
        CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;

        await tagRepository.DeleteTagConnectionAsync(tagId, publicationId, ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> SaveGeneratedTags(string userId, string publicationId, List<GeneratedTagDto> tags, 
        CancellationToken ct)
    {
        var authorizationResult = await VerifyAuthorizationRules(userId, publicationId, ct);
        if (!authorizationResult.IsSuccess)
            return authorizationResult;
        
        var amountValidationResult = await VerifyTagLimit(tags.Count, publicationId, ct);
        if (!amountValidationResult.IsSuccess)
            return amountValidationResult;
        
        var tagEntities = tags.Select(t => new Tag { Name = t.Name }).ToList();

        var (existingTags, newTags) = await tagRepository.SplitExistingAndNewTagsAsync(tagEntities, ct); 
        
        if (newTags.Count > 0)
            await tagRepository.AddTagsAsync(newTags, ct);
        
        var allTags = existingTags.Concat(newTags).ToList();

        await tagRepository.AddTagsToPublicationAsync(publicationId, allTags, ct);

        return Result<Unit>.Success(Unit.Value);
    }

    private async Task<Result<Unit>> VerifyAuthorizationRules(string userId, string publicationId, CancellationToken ct)
    {
        if (!await publicationRepository.IsPublicationExistsAsync(publicationId, ct))
            return Result<Unit>.Failure(PublicationErrors.NotFound());

        if (!await publicationRepository.IsUserAuthorAsync(userId, publicationId, ct))
            return Result<Unit>.Failure(PublicationErrors.NotAuthorised());
        return Result<Unit>.Success(Unit.Value);
    }

    private async Task<Result<Unit>> VerifyTagLimit(int tagsAmount, string publicationId, CancellationToken ct)
    {
        var amount = await tagRepository.GetTagsAmountByPublicationIdAsync(publicationId, ct);
        if (amount == -1)
            return Result<Unit>.Failure(PublicationErrors.NotFound());

        if (amount + tagsAmount > 20)
            return Result<Unit>.Failure(TagErrors.TagAmountLimitViolation());
        return Result<Unit>.Success(Unit.Value);
    }
}