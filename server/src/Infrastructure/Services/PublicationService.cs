using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Application.DTOs.UserDTOs;
using Application.Features.Images.Commands;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PublicationService(ApplicationDbContext context, IMapper mapper, ILikeService likeService,
    IUserActionLogger<PublicationService> logger, ICloudinaryService cloudinaryService, ISpamRepository 
        spamRepository, IPhotoService photoService) 
    : IPublicationService
{
    // normal user
    private const int PublicationTimeLimit = 5; // in minutes
    private const int PublicationNumberLimit = 50;
    
    // new user
    private const int NewUserPublicationNumberLimit = 1;
    private const int NewUserPublicationTimeLimit = 10; // in minutes

    public async Task<Result<List<PublicationNotificationDto>>> GetPublicationsToRemindAsync(DateTime postedAt, int 
            batchSize, CancellationToken ct)
    {
        DateTime now = DateTime.UtcNow;
        return Result<List<PublicationNotificationDto>>.Success(
            await context.Publications
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
                .ToListAsync(ct)
        );
    }

    public async Task<Result<List<PublicationCalendarDto>>> GetPublicationsCalendarAsync(string userId, 
        CancellationToken ct)
    {
        var publications = await context.Publications
            .Where(a => a.AuthorId == userId && a.PublicationType == PublicationTypes.planned)
            .ProjectTo<PublicationCalendarDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return Result<List<PublicationCalendarDto>>.Success(publications);
    }

    public async Task<Result<PagedList<PublicationCardDto>>> GetPublicationsByUniqueNameIdentifierAsync(
        string uniqueName, string userId, int page, int pageSize, CancellationToken ct)
    {
        var userExists = await context.Users
            .AnyAsync(u => u.Id == userId, ct);

        if (!userExists)
            return Result<PagedList<PublicationCardDto>>
                .Failure("User was not found", 404);
        
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

        return Result<PagedList<PublicationCardDto>>
            .Success(pagedResult);
    }

    public async Task<Result<PublicationDto>> GetPublicationByIdAsync(string publicationId, string userId, 
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

        if (publication == null)
            return Result<PublicationDto>.Failure("Publication was not found", 404);

        return Result<PublicationDto>.Success(publication);
    }

    public async Task<Result<List<PublicationDto>>> GetPlannedPublicationsAsync(string userId, CancellationToken ct)
    {
        var publications = await context.Publications
            .Where(a => a.RemindAt != null && a.AuthorId == userId)
            .ToListAsync(ct);

        var mapped = mapper.Map<List<PublicationDto>>(publications);
        return Result<List<PublicationDto>>.Success(mapped);
    }

    public async Task<Result<int>> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct)
    {
        await context.Publications.Where(p => p.Id == publicationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);

        var viewCount = await context.Publications.Where(a => a.Id == publicationId)
            .Select(x => x.ViewCount).FirstOrDefaultAsync(ct);

        return Result<int>.Success(viewCount);
    }

    public async Task<Result<PublicationDto>> UpdatePublicationAsync(UpdatePublicationDto updateDto, string userId, 
        CancellationToken ct)
    {
        var publication = await context.Publications
        .Where(p => p.Id == updateDto.Id)
        .Select(p => new
        {
            p.Id,
            p.AuthorId,
            p.PublicationType,
            p.RemindAt
        })
        .FirstOrDefaultAsync(ct);

        if (publication == null)
            return Result<PublicationDto>.Failure("Publication was not found", 404);

        await context.Publications
            .Where(p => p.Id == updateDto.Id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Content, updateDto.Content)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                ct);

        if (publication.PublicationType == PublicationTypes.planned
            && updateDto.RemindAt > DateTime.UtcNow
            && publication.RemindAt != updateDto.RemindAt)
        {
            await context.Publications
                .Where(p => p.Id == updateDto.Id)
                .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.RemindAt, updateDto.RemindAt), ct);
        }
        
        var success = await context.SaveChangesAsync(ct) > 0;
        if (!success) return Result<PublicationDto>.Failure("Publication was not updated", 500);
        
        var readyPublication = await context.Publications
            .Where(p => p.Id == updateDto.Id)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                Content = p.Content,
                PostedAt = p.PostedAt,
                UpdatedAt = p.UpdatedAt,
                RemindAt = p.RemindAt,
                LikesAmount = p.Likes.Count,
                CommentAmount = p.Comments == null ? 0 : p.Comments.Count,
                PublicationType = p.PublicationType,
                ConditionType = p.ConditionType,
                ConditionTarget = p.ConditionTarget,
                Author = new PublicUserBriefDto
                {
                    Id = p.Author.Id,
                    Username = p.Author.Username,
                    UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                    Blocked = p.Author.Blocked,
                    ImageUrl = p.Author.ProfileImage == null ? null : p.Author.ProfileImage.ImageUrl
                },
                ComparisonOperator = p.ComparisonOperator,
                ViewCount = p.ViewCount,
                IsDeleted = p.IsDeleted,
                Images = p.Images!
                    .Select(x => x.ImageUrl)
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        var isLikedResult = await likeService.IsLikedByAsync(userId, publication.Id, ct);
        if (isLikedResult.IsSuccess)
        {
            readyPublication.IsLikedByCurrentUser = isLikedResult.Value;
        }
        else
        {
            return Result<PublicationDto>.Failure(isLikedResult.Error!, isLikedResult.Code);
        }
        await logger.LogAsync(userId, UserLogAction.EditPublication, new
        {
            info = $"User {userId} has updated publication {publication.Id}"
        }, publication.Id, ct);
        return Result<PublicationDto>.Success(readyPublication);
    }

    public async Task<Result<Unit>> SetPublicationSentStateAsync(string publicationId, bool state, CancellationToken ct)
    {
        bool exists = await context.Publications.AnyAsync(c => c.Id == publicationId, ct);
        if (!exists)
            return Result<Unit>.Failure("Publication was not found", 404);

        await context.Publications.Where(a => a.Id == publicationId).ExecuteUpdateAsync(c => c.SetProperty(x => x
            .WasSent, state), ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> DeletePublicationAsync(string publicationId, CancellationToken ct)
    {
        var publication = await context.Publications
            .Select(p => new
            {
                p.Id,
                p.AuthorId,
                ImagePublicIds = p.Images != null ? p.Images.Select(i => i.PublicId).ToList() : null
            }).FirstOrDefaultAsync(a => a.Id == publicationId, ct);

        if (publication == null)
            return Result<Unit>.Failure("Publication was not deleted because it does not exist", 404);


        await context.Publications
            .Where(p => p.Id == publicationId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), ct);
        if (publication.ImagePublicIds is { Count: > 0 })
        {
            await Task.WhenAll(publication.ImagePublicIds
                .Select(id => cloudinaryService.DeletePhotoAsync(id)));
        }
            
        await logger.LogAsync(publication.AuthorId, UserLogAction.DeletePublication, new
        {
            info = $"Publication " +
                   $"was deleted by {publication.AuthorId}"
        }, publication.Id, ct);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<bool>> CreatePublicationAsync(CreatePublicationDto createDto, string creatorId, 
        CancellationToken ct)
    {
        User? user = await context.Users.FindAsync(creatorId, ct);
        if (user == null)
            return Result<bool>
                .Failure("Account does not exist therefore publication cannot be created", 403);

        bool isPublicationSpamming = await IsPublicationSpamming(user.Id, user.DateOfCreation, ct);
        if (isPublicationSpamming)
            return Result<bool>
                .Failure("You cannot make that many publications in short period of time", 400);

        var res = await spamRepository.MakePublication(user.Id, ct);
        if (!res)
        {
            return Result<bool>.Failure(
                "You cannot make more publications for today due to our anti spam rules", 400);
        }
            
        Publication publication = mapper.Map<Publication>(createDto);
        publication.Author = user;
        publication.AuthorId = user.Id;

        if (createDto.Images != null && createDto.Images.Count > 0)
        {
            var images = await photoService.UploadPublicationImages(createDto.Images, ct);
            if (images.IsSuccess)
            {
                publication.Images = images.Value;
            }
            else
            {
                return Result<bool>.Failure(images.Error!, images.Code);
            }

        }

        context.Add(publication);

        await logger.LogAsync(creatorId, UserLogAction.CreatePublication, new
        {
            info = $"User {createDto} has created a publication {publication.Id}"
        }, publication.Id, ct);
            
        return Result<bool>.Success(true);
    }
    
    private async Task<bool> IsPublicationSpamming(string userId, DateOnly userCreationDate, CancellationToken ct)
    {
        bool isUserNew = userCreationDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            
        // Set limits based on user status
        int timeLimitMinutes = isUserNew ? NewUserPublicationTimeLimit : PublicationTimeLimit;
        int? maxPublications = isUserNew ? NewUserPublicationNumberLimit : PublicationNumberLimit;

        // BUG FIX: Subtract minutes to look into the past, not the future
        var cutOffTime = DateTime.UtcNow.AddMinutes(-timeLimitMinutes);

        // Run a single, clean query
        int recentPublicationCount = await context.Publications
            .CountAsync(a => a.AuthorId == userId && a.PostedAt >= cutOffTime, ct);

        return recentPublicationCount >= maxPublications;
    }
}