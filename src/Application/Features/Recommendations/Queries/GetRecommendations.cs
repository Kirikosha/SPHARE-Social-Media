using AutoMapper;

namespace Application.Features.Recommendations.Queries;

using Core;
using Domain.DTOs.PublicationDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetRecommendations
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required string UserId { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var interests = await context.UserInterestTags
                .Where(i => i.UserId == request.UserId && i.Weight > 0.1f)
                .ToListAsync(cancellationToken);

            if (!interests.Any())
            {
                var publications = await GetFallbackRecommendationsAsync(request.Page, request.PageSize,
                    cancellationToken);

                return Result<List<PublicationDto>>.Success(mapper.Map<List<PublicationDto>>(publications));
            }
            var interestTagIds = interests.Select(i => i.TagId).ToHashSet();

            var seenIds = await context.PublicationViews
                .Where(v => v.UserId == request.UserId 
                            && v.ViewedAt > DateTime.UtcNow.AddDays(-30))
                .Select(v => v.PublicationId)
                .ToListAsync(cancellationToken);

            var candidates = await context.Publications
                .Where(p => !p.IsDeleted
                            && !seenIds.Contains(p.Id)
                            && p.PublicationTags.Any(pt => interestTagIds.Contains(pt.TagId)))
                .Select(p => new
                {
                    Publication = p,
                    MatchingTagIds = p.PublicationTags
                        .Where(pt => interestTagIds.Contains(pt.TagId))
                        .Select(pt => pt.TagId)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var weightLookup = interests.ToDictionary(i => i.TagId, i => i.Weight);

            var skip = request.Page * request.PageSize;

            var scored = candidates
                .Select(c => new
                {
                    c.Publication,
                    Score = CalculateScore(c.MatchingTagIds, weightLookup, c.Publication)
                })
                .OrderByDescending(x => x.Score)
                .Skip(skip)
                .Take(request.PageSize)
                .Select(x => x.Publication)
                .ToList();

            return Result<List<PublicationDto>>.Success(mapper.Map<List<PublicationDto>>(scored));
 
            
        }
        private async Task<List<Publication>> GetFallbackRecommendationsAsync(
            int page, int pageSize, CancellationToken ct)
        {
            return await context.Publications
                .Where(p => !p.IsDeleted && p.PostedAt > DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(p => p.Likes.Count * 0.6f + p.ViewCount * 0.1f)
                .Skip(page * pageSize)
                .Take(pageSize)
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
