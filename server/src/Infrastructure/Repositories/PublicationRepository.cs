using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Publications;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PublicationRepository(ApplicationDbContext context, IMapper mapper) : IPublicationRepository
{
    public async Task<List<PublicationNotificationDto>> GetPublicationsToRemindAsync(
        DateTime postedAt, int batchSize, CancellationToken ct)
    {
        DateTime now = DateTime.UtcNow;

        var plannedQuery = context.PlannedPublications
            .AsNoTracking()
            .Where(p => p.PublishAt <= now && !p.WasPublished && p.PostedAt > postedAt)
            .Select(p => new PublicationNotificationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                WasSent = p.WasPublished,
                AuthorId = p.AuthorId,
                AuthorUsername = p.Author.Username,
                AuthorImageUrl = p.Author.ProfileImage != null ? p.Author.ProfileImage.ImageUrl : null,
                FirstImageUrl = p.Images!.Select(i => i.ImageUrl).FirstOrDefault()
            });

        var conditionalQuery = context.ConditionalPublications
            .AsNoTracking()
            .Where(p => !p.WasPublished
                        && p.PostedAt > postedAt
                        && p.ComparisonOperator == ComparisonOperator.GreaterThanOrEqual
                        && p.Author.SubscriberNumber >= p.ConditionTarget)
            .Select(p => new PublicationNotificationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                WasSent = p.WasPublished,
                AuthorId = p.AuthorId,
                AuthorUsername = p.Author.Username,
                AuthorImageUrl = p.Author.ProfileImage != null ? p.Author.ProfileImage.ImageUrl : null,
                FirstImageUrl = p.Images!.Select(i => i.ImageUrl).FirstOrDefault()
            });

        return await plannedQuery
            .Concat(conditionalQuery)
            .OrderBy(p => p.PostedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<List<PublicationCalendarDto>> GetPublicationsForCalendarAsync(
        string userId, CancellationToken ct)
    {
        return await context.PlannedPublications
            .AsNoTracking()
            .Where(p => p.AuthorId == userId)
            .Select(p => new PublicationCalendarDto
            {
                Id = p.Id,
                Content = p.Content,
                PublishAt = p.PublishAt
            })
            .ToListAsync(ct);
    }

    public async Task<PagedList<PublicationCardDto>> GetBatchOfPublicationCardsDto(
        string uniqueName, string userId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Publications
            .AsNoTracking()
            .Where(p => p.Author.UniqueNameIdentifier == uniqueName)
            .OrderByDescending(p => p.PostedAt)
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
                IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == userId),
                CommentAmount = p.Comments!.Count(),
                PublicationType = p is PlannedPublication ? PublicationTypes.planned
                    : p is ConditionalPublication ? PublicationTypes.conditional
                    : PublicationTypes.ordinary,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted
            });

        return await query.ToPagedListAsync(page, pageSize, ct);
    }

    public async Task<PublicationDto?> GetPublicationByIdAsync(
        string publicationId, string userId, CancellationToken ct)
    {
        return await context.Publications
            .AsNoTracking()
            .Where(p => p.Id == publicationId)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted,
                LikesAmount = p.Likes.Count,
                CommentAmount = p.Comments!.Count(c => c.ParentCommentId == null),
                IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == userId),
                Images = p.Images!.Select(i => i.ImageUrl).ToList(),
                PublicationType = p is PlannedPublication ? PublicationTypes.planned
                    : p is ConditionalPublication ? PublicationTypes.conditional
                    : PublicationTypes.ordinary,
                PublishAt = (p as PlannedPublication).PublishAt,
                ConditionType = (p as ConditionalPublication).ConditionType,
                ConditionTarget = (p as ConditionalPublication).ConditionTarget,
                ComparisonOperator = (p as ConditionalPublication).ComparisonOperator,
                Author = new PublicUserBriefDto
                {
                    Id = p.Author.Id,
                    Username = p.Author.Username,
                    UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                    Blocked = p.Author.Blocked,
                    ImageUrl = p.Author.ProfileImage == null ? null : p.Author.ProfileImage.ImageUrl
                }
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Publication?> GetRawPublicationByIdAsync(string publicationId, CancellationToken ct)
    {
        return await context.Publications.Where(p => p.Id == publicationId).FirstOrDefaultAsync(ct);
    }

    public async Task<List<PublicationDto>> GetPlannedPublicationsAsync(string userId, CancellationToken ct)
    {
        return await context.PlannedPublications
            .AsNoTracking()
            .Where(p => p.AuthorId == userId)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                PublishAt = p.PublishAt,
                PublicationType = PublicationTypes.planned,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted,
                LikesAmount = p.Likes.Count,
                CommentAmount = p.Comments!.Count(c => c.ParentCommentId == null),
                IsLikedByCurrentUser = p.Likes.Any(l => l.LikedById == userId),
                Images = p.Images!.Select(i => i.ImageUrl).ToList(),
                Author = new PublicUserBriefDto
                {
                    Id = p.Author.Id,
                    Username = p.Author.Username,
                    UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                    Blocked = p.Author.Blocked,
                    ImageUrl = p.Author.ProfileImage == null ? null : p.Author.ProfileImage.ImageUrl
                }
            })
            .ToListAsync(ct);
    }

    public async Task<int> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct)
    {
        await context.Publications.Where(p => p.Id == publicationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);

        var viewCount = await context.Publications.Where(a => a.Id == publicationId)
            .Select(x => x.ViewCount).FirstOrDefaultAsync(ct);
        return viewCount;
    }

    public async Task<bool> IsPublicationExistsAsync(string id, CancellationToken ct)
    {
        return await context.Publications
            .AnyAsync(a => a.Id == id, ct);
    }

    public async Task<bool> IsUserAuthorAsync(string userId, string publicationId, CancellationToken ct)
    {
        return await context.Publications.AnyAsync(x => x.Id == publicationId && x.AuthorId == userId, ct);
    }

    public async Task<bool> UpdatePublicationContentAsync(UpdatePublicationContentDto updateContent, 
        CancellationToken ct)
    {
        int rowsAffected = await context.Publications.Where(p => p.Id == updateContent.PublicationId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Content, updateContent.NewContent)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct);

        return rowsAffected == 1;
    }

    public async Task<bool> SetPublicationStateToSentAsync(string publicationId, CancellationToken ct)
    {
        int rowsAffected = await context.Publications.Where(a => a.Id == publicationId).ExecuteUpdateAsync(c => c
            .SetProperty(x => x
            .WasPublished, true), ct);

        return rowsAffected > 0;
    }

    public async Task<PublicationNavigationProperties?> GetPublicationNavigationPropertiesAsync(string publicationId, 
        CancellationToken ct)
    {
        var navigationProps = await context.Publications
            .Select(p => new PublicationNavigationProperties
            {
                PublicationId = p.Id,
                AuthorId = p.AuthorId,
                ImageIds = p.Images != null ? p.Images.Select(i => i.PublicId).ToList() : null
            }).FirstOrDefaultAsync(a => a.PublicationId == publicationId, ct);

        return navigationProps;
    }

    public async Task DeletePublicationAsync(string publicationId, CancellationToken ct)
    {
        await context.Publications
            .Where(p => p.Id == publicationId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), ct);
    }
}