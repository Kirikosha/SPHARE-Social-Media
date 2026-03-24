using Domain.DTOs.UserDTOs;

namespace Application.Features.Recommendations.Queries;

using Core;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRecommendations
{
    public class Query : IRequest<Result<List<PublicationCardDto>>>
    {
        public required string UserId { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
    }

    public class Handler(ApplicationDbContext context)
        : IRequestHandler<Query, Result<List<PublicationCardDto>>>
    {
        public async Task<Result<List<PublicationCardDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            
            var interests = await context.UserInterestTags
                .AsNoTracking()
                .Where(i => i.UserId == request.UserId && i.Weight > 0.1f)
                .ToListAsync(cancellationToken);

            if (!interests.Any())
            {
                var fallbackDtos = await GetFallbackRecommendationsAsync(
                    request, cancellationToken);

                return Result<List<PublicationCardDto>>.Success(fallbackDtos);
            }

            var interestTagIds = interests.Select(i => i.TagId).ToHashSet();
            var weightLookup = interests.ToDictionary(i => i.TagId, i => i.Weight);

            var seenIds = await context.PublicationViews
                .AsNoTracking()
                .Where(v => v.UserId == request.UserId &&
                            v.ViewedAt > now.AddDays(-30))
                .Select(v => v.PublicationId)
                .ToListAsync(cancellationToken);

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
                .ToListAsync(cancellationToken);
 
            var scored = candidates
                .Select(c => new
                {
                    c.Publication,
                    Score = CalculateScore(c.MatchingTagIds, weightLookup, c.Publication)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var skip = request.Page * request.PageSize;

            var recommended = scored
                .Skip(skip)
                .Take(request.PageSize)
                .Select(x => x.Publication)
                .ToList();
            
            if (recommended.Count < request.PageSize)
            {
                var fallback = await context.Publications
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && p.PostedAt > now.AddDays(-7))
                    .OrderByDescending(p => p.Likes.Count * 0.6f + p.ViewCount * 0.1f)
                    .Skip(skip)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var needed = request.PageSize - recommended.Count;

                var additional = fallback
                    .Where(f => recommended.All(r => r.Id != f.Id))
                    .Take(needed)
                    .ToList();

                recommended.AddRange(additional);
            }
            
            var result = recommended
                .Select(p => new PublicationCardDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,
                    ImageUrls = p.Images!
                        .Select(i => i.ImageUrl)
                        .ToList(),
                    Author = new PublicUserBriefDto
                    {
                        Id = p.Author.Id,
                        Username = p.Author.Username,
                        UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                        Blocked = p.Author.Blocked,
                        ImageUrl = p.Author.ProfileImage != null
                            ? p.Author.ProfileImage.ImageUrl
                            : null
                    },
                    LikesAmount = p.Likes.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == request.UserId),
                    CommentAmount = p.Comments!.Count(),
                    PublicationType = p.PublicationType,
                    ViewCount = p.ViewCount,
                    IsDeleted = p.IsDeleted
                })
                .ToList();

            return Result<List<PublicationCardDto>>.Success(result);
        }
        private async Task<List<PublicationCardDto>> GetFallbackRecommendationsAsync(
            Query request,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return await context.Publications
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.PostedAt > now.AddDays(-7))
                .OrderByDescending(p => p.Likes.Count * 0.6f + p.ViewCount * 0.1f)
                .Skip(request.Page * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PublicationCardDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,

                    ImageUrls = p.Images!
                        .Select(i => i.ImageUrl)
                        .ToList(),

                    Author = new PublicUserBriefDto
                    {
                        Id = p.Author.Id,
                        Username = p.Author.Username,
                        UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                        Blocked = p.Author.Blocked,
                        ImageUrl = p.Author.ProfileImage != null
                            ? p.Author.ProfileImage.ImageUrl
                            : null
                    },

                    LikesAmount = p.Likes.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == request.UserId),
                    CommentAmount = p.Comments!.Count(),

                    PublicationType = p.PublicationType,
                    ViewCount = p.ViewCount,
                    IsDeleted = p.IsDeleted
                })
                .ToListAsync(ct);
        }
        
        private float CalculateScore(
            List<int> matchingTagIds, 
            Dictionary<int, float> weightLookup, 
            Publication publication)
        {
            float tagScore = matchingTagIds
                .Sum(tagId => weightLookup.GetValueOrDefault(tagId, 0f));

            float ageInDays = (float)(DateTime.UtcNow - publication.PostedAt).TotalDays;
            float recencyScore = 1f / (1f + ageInDays * 0.1f);

            float popularityScore = MathF.Log(1 + publication.ViewCount) * 0.05f;

            return (tagScore * 0.70f) + (recencyScore * 0.20f) + (popularityScore * 0.10f);
        }
    }
}
