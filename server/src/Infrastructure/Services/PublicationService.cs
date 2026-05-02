using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.PublicationDTOs;
using Application.Errors;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Publications;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PublicationService(ApplicationDbContext context, IPublicationRepository publicationRepository, 
    IMapper mapper, IUserActionLogger<PublicationService> logger,
    ICloudinaryService cloudinaryService, ISpamRepository spamRepository, IPhotoService photoService,
    IUserRepository userRepository) 
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
        try
        {
            var publications = await publicationRepository.GetPublicationsToRemindAsync(postedAt, batchSize, ct);
            return Result<List<PublicationNotificationDto>>.Success(publications);
        }
        catch
        {
            return Result<List<PublicationNotificationDto>>.Failure("Publication fetching was unsuccessful", 500);
        }
    }

    public async Task<Result<List<PublicationCalendarDto>>> GetPublicationsCalendarAsync(string userId, 
        CancellationToken ct)
    {
        bool userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<List<PublicationCalendarDto>>
                .Failure("User was not found", 404);
        try
        {
            var publications = await publicationRepository.GetPublicationsForCalendarAsync(userId, ct);
            return Result<List<PublicationCalendarDto>>.Success(publications);
        }
        catch
        {
            return Result<List<PublicationCalendarDto>>.Failure("Calendar fetching was unsuccessful", 500);
        }

    }

    public async Task<Result<PagedList<PublicationCardDto>>> GetPublicationsByUniqueNameIdentifierAsync(
        string uniqueName, string userId, int page, int pageSize, CancellationToken ct)
    {
        bool userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<PagedList<PublicationCardDto>>
                .Failure("User was not found", 404);

        try
        {
            var pagedResult = await publicationRepository.GetBatchOfPublicationCardsDto(uniqueName, userId, page,
                pageSize, ct);
            return Result<PagedList<PublicationCardDto>>
                .Success(pagedResult);
        }
        catch
        {
            return Result<PagedList<PublicationCardDto>>
                .Failure("Publication loading was unsuccessful", 400);
        }
    }

    public async Task<Result<PublicationDto>> GetPublicationByIdAsync(string publicationId, string userId, 
        CancellationToken ct)
    {
        bool userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<PublicationDto>
                .Failure("User was not found", 404);
        
        var publication = await publicationRepository.GetPublicationByIdAsync(publicationId, userId, ct);

        if (publication == null)
            return Result<PublicationDto>.Failure("Publication was not found", 404);

        return Result<PublicationDto>.Success(publication);
    }

    public async Task<Result<List<PublicationDto>>> GetPlannedPublicationsAsync(string userId, CancellationToken ct)
    {
        bool userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<List<PublicationDto>>
                .Failure("User was not found", 404);
        try
        {
            var publications = await publicationRepository.GetPlannedPublicationsAsync(userId, ct);
            return Result<List<PublicationDto>>.Success(publications);
        }
        catch
        {
            return Result<List<PublicationDto>>.Failure("Fetching planned publications was unsuccessful", 500);
        }
    }

    public async Task<Result<int>> UpdatePublicationViewsAsync(string publicationId, CancellationToken ct)
    {
        try
        {
            var viewCount = await publicationRepository.UpdatePublicationViewsAsync(publicationId, ct);
            return Result<int>.Success(viewCount);
        }
        catch
        {
            return Result<int>.Failure("Publication view update was unsuccessful", 500);
        }
    }


    public async Task<Result<PublicationDto>> UpdatePublicationContentAsync(UpdatePublicationContentDto 
            updateContentDto,
        string userId, CancellationToken ct)
    {
        var userAllowed = await publicationRepository.IsUserAuthorAsync(userId, updateContentDto.PublicationId, ct);
        if (!userAllowed)
            return Result<PublicationDto>.Failure(
                PublicationErrors.NotAuthorised());

        try
        {
            await publicationRepository.UpdatePublicationContentAsync(updateContentDto, ct);

            var publication = await publicationRepository.GetPublicationByIdAsync(updateContentDto.PublicationId,
                userId, ct);

            return publication == null
                ? Result<PublicationDto>.Failure(PublicationErrors.FetchingUnsuccessful())
                : Result<PublicationDto>.Success(publication);
        }
        catch
        {
            return Result<PublicationDto>.Failure(PublicationErrors.UpdateUnsuccessful());
        }
        
    }

    public Task<Result<PublicationDto>> UpdateConditionalPublicationAsync(UpdateConditionalPublicationDto updateDto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PublicationDto>> UpdatePlannedPublicationAsync(UpdatePlannedPublicationDto updateDto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Unit>> SetPublicationSentStateAsync(string publicationId, bool state, CancellationToken ct)
    {
        var isSuccess = await publicationRepository.SetPublicationStateToSentAsync(publicationId, ct);
        return !isSuccess ? Result<Unit>.Failure(PublicationErrors.NotFound()) : Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> DeletePublicationAsync(string publicationId, string userId, CancellationToken ct)
    {
        var publicationNavProps = await publicationRepository
            .GetPublicationNavigationPropertiesAsync(publicationId, ct);

        if (publicationNavProps == null)
            return Result<Unit>.Failure(PublicationErrors.NotFound());

        if (userId != publicationNavProps.AuthorId)
            return Result<Unit>.Failure(PublicationErrors.NotAuthorised());

        await publicationRepository.DeletePublicationAsync(publicationId, ct);
        
        if (publicationNavProps.ImageIds is { Count: > 0 })
        {
            var deletionResult = await photoService.DeletePublicationImagesAsync(publicationNavProps.ImageIds, ct);
            if (!deletionResult.IsSuccess) return deletionResult;
        }
            
        //TODO: Fix the logging
        /*
        await logger.LogAsync(publication.AuthorId, UserLogAction.DeletePublication, new
        {
            info = $"Publication " +
                   $"was deleted by {publication.AuthorId}"
        }, publication.Id, ct);
        */
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