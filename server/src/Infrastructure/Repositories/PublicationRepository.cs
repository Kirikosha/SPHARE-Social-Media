using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PublicationRepository(ApplicationDbContext context, IMapper mapper) : IPublicationRepository
{
    public async Task<List<PublicationNotificationDto>> GetPublicationsToRemindAsync(DateTime postedAt, int batchSize, 
        CancellationToken ct)
    {
        DateTime now = DateTime.UtcNow;
        return await context.Publications
            .AsNoTracking()
            .Where(p =>
                ((p.RemindAt != null && p.RemindAt <= now) ||
                 (p.ConditionType != null &&
                  p.ComparisonOperator == ComparisonOperator.GreaterThanOrEqual &&
                  p.Author.SubscriberNumber >= p.ConditionTarget))
                && !p.WasSent
                && p.PostedAt > postedAt)
            .OrderBy(p => p.PostedAt)
            .Select(p => new PublicationNotificationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                WasSent = p.WasSent,
                AuthorId = p.AuthorId,

                AuthorUsername = p.Author.Username,
                AuthorImageUrl = p.Author.ProfileImage != null
                    ? p.Author.ProfileImage.ImageUrl
                    : null,
                FirstImageUrl = p.Images!
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<List<PublicationCalendarDto>> GetPublicationsForCalendarAsync(string userId, CancellationToken ct)
    {
        var publications = await context.Publications
            .Where(a => a.AuthorId == userId && a.PublicationType == PublicationTypes.planned)
            .ProjectTo<PublicationCalendarDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return publications;
    }

    public async Task<PagedList<PublicationCardDto>> GetBatchOfPublicationCardsDto(string uniqueName, string userId, int 
            page, int pageSize, CancellationToken ct)
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

                IsLikedByCurrentUser = p.Likes
                    .Any(l => l.LikedById == userId),

                CommentAmount = p.Comments!.Count(),

                PublicationType = p.PublicationType,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted
            });

        var pagedResult = await query.ToPagedListAsync(page, pageSize, ct);
        return pagedResult;
    }

    public async Task<PublicationDto?> GetPublicationByIdAsync(string publicationId, string userId,
        CancellationToken ct)
    {
        var publication = await context.Publications
            .Where(p => p.Id == publicationId)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                RemindAt = p.RemindAt,
                PublicationType = p.PublicationType,
                ConditionType = p.ConditionType,
                ConditionTarget = p.ConditionTarget,
                ComparisonOperator = p.ComparisonOperator,
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
            .FirstOrDefaultAsync(ct);
        return publication;
    }

    public async Task<List<PublicationDto>> GetPlannedPublicationsAsync(string userId, CancellationToken ct)
    {
        var publications = await context.Publications
            .Where(a => a.RemindAt != null && a.AuthorId == userId)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                RemindAt = p.RemindAt,
                PublicationType = p.PublicationType,
                ConditionType = p.ConditionType,
                ConditionTarget = p.ConditionTarget,
                ComparisonOperator = p.ComparisonOperator,
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

        return mapper.Map<List<PublicationDto>>(publications);
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
}