using Domain.Entities.Publications;

namespace Infrastructure.Services;
using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;
using Domain.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;


public class RecommendationService(ApplicationDbContext context) : IRecommendationService
{
    public async Task<Result<PagedList<PublicationCardDto>>> GetRecommendations(string userId, int page, int pageSize,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var interests = await context.UserInterestTags
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Weight > 0.1f)
            .ToListAsync(ct);

        if (!interests.Any())
        {
            var fallback = await GetFallbackRecommendationsAsync(userId, page, pageSize, ct);
            return Result<PagedList<PublicationCardDto>>.Success(fallback);
        }

        var interestTagIds = interests.Select(i => i.TagId).ToHashSet();
        var weightLookup = interests.ToDictionary(i => i.TagId, i => i.Weight);

        var seenIds = await context.PublicationViews
            .AsNoTracking()
            .Where(v => v.UserId == userId && v.ViewedAt > now.AddDays(-30))
            .Select(v => v.PublicationId)
            .ToListAsync(ct);

        var candidates = await context.Publications
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                        && !seenIds.Contains(p.Id)
                        && p.PublicationTags.Any(pt => interestTagIds.Contains(pt.TagId)))
            .OrderByDescending(p => p.PostedAt)
            .Take(200)
            .Select(p => new
            {
                Publication = p,
                MatchingTagIds = p.PublicationTags
                    .Where(pt => interestTagIds.Contains(pt.TagId))
                    .Select(pt => pt.TagId)
                    .ToList()
            })
            .ToListAsync(ct);

        var scored = candidates
            .Select(c => new
            {
                c.Publication,
                Score = CalculateScore(c.MatchingTagIds, weightLookup, c.Publication)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var skip = (page - 1) * pageSize;

        var recommended = scored
            .Skip(skip)
            .Take(pageSize)
            .Select(x => x.Publication)
            .ToList();

        if (recommended.Count < pageSize)
        {
            var fallbackItems = await context.Publications
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.PostedAt > now.AddDays(-7))
                .OrderByDescending(p => p.Likes.Count * 0.6f + p.ViewCount * 0.1f)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            var needed = pageSize - recommended.Count;
            var additional = fallbackItems
                .Where(f => recommended.All(r => r.Id != f.Id))
                .Take(needed)
                .ToList();

            recommended.AddRange(additional);
        }

        var resultItems = recommended
            .Select(p => MapToDto(p, userId))
            .ToList();

        // Total count is the full scored pool size for accurate pagination metadata
        var pagedResult = PagedList<PublicationCardDto>.Create(resultItems, scored.Count, page, pageSize);

        return Result<PagedList<PublicationCardDto>>.Success(pagedResult);
    }

    private async Task<PagedList<PublicationCardDto>> GetFallbackRecommendationsAsync(
        string userId, int page, int pageSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Pure IQueryable — ToPagedListAsync works perfectly here
        return await context.Publications
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.PostedAt > now.AddDays(-7))
            .OrderByDescending(p => p.Likes.Count * 0.6f + p.ViewCount * 0.1f)
            .Select(p => new PublicationCardDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                ImageUrls = p.Images!.Select(i => i.ImageUrl).ToList(),
                Author = new PublicUserBriefDto
                {
                    Id = p.Author.Id,
                    Username = p.Author.Username,
                    UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                    Blocked = p.Author.Blocked,
                    ImageUrl = p.Author.ProfileImage != null ? p.Author.ProfileImage.ImageUrl : null
                },
                LikesAmount = p.Likes.Count(),
                IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == userId),
                CommentAmount = p.Comments!.Count(),
                PublicationType = p.PublicationType,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted
            })
            .ToPagedListAsync(page, pageSize, ct);
    }

    private static PublicationCardDto MapToDto(Publication p, string userId) => new()
    {
        Id = p.Id,
        Content = p.Content,
        PostedAt = p.PostedAt,
        UpdatedAt = p.UpdatedAt,
        ImageUrls = p.Images!.Select(i => i.ImageUrl).ToList(),
        Author = new PublicUserBriefDto
        {
            Id = p.Author.Id,
            Username = p.Author.Username,
            UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
            Blocked = p.Author.Blocked,
            ImageUrl = p.Author.ProfileImage != null ? p.Author.ProfileImage.ImageUrl : null
        },
        LikesAmount = p.Likes.Count(),
        IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == userId),
        CommentAmount = p.Comments!.Count(),
        PublicationType = p.PublicationType,
        ViewCount = p.ViewCount,
        IsDeleted = p.IsDeleted
    };

    private float CalculateScore(
        List<int> matchingTagIds,
        Dictionary<int, float> weightLookup,
        Publication publication)
    {
        float tagScore = matchingTagIds.Sum(tagId => weightLookup.GetValueOrDefault(tagId, 0f));
        float ageInDays = (float)(DateTime.UtcNow - publication.PostedAt).TotalDays;
        float recencyScore = 1f / (1f + ageInDays * 0.1f);
        float popularityScore = MathF.Log(1 + publication.ViewCount) * 0.05f;
        return (tagScore * 0.70f) + (recencyScore * 0.20f) + (popularityScore * 0.10f);
    }
}