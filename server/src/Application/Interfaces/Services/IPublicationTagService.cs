using Application.Core;
using Application.DTOs.TagDTOs;
using Domain.Entities.RecomendationEntities;

namespace Application.Interfaces.Services;

public interface IPublicationTagService
{
    Task<Result<List<GeneratedTagDto>>> GenerateTags(string publicationId, string userId, CancellationToken ct);
}