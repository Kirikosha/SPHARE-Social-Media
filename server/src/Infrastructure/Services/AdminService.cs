using Application.Core;
using Application.DTOs.ViolationDTOs;
using Application.Interfaces.Logger;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public class AdminService(IUserRepository userRepository, IViolationNotificationService violationNotificationService,
    ICommentRepository commentRepository, IPublicationRepository publicationRepository, 
    IUserActionLogger<AdminService> logger) 
    : IAdminService
{
    public async Task<Result<bool>> UnblockUserAsync(string userId, CancellationToken ct)
    {
        try
        {
            var userResult = await userRepository.IsUserExistsByIdAsync(userId, ct);
            if (!userResult)
                return Result<bool>.Failure("User does not exist", 404);

            var updateResult = await userRepository.UnBlockUserAsync(userId, ct);
            if (updateResult!)
                return Result<bool>.Failure("User unblocking was unsucessful", 500);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Something went wrong during the process. Error: {ex.Message}", 500);
        }
    }
    
    public async Task<Result<bool>> BlockUserAsync(string userId, string blockedById, CancellationToken ct)
    {
        try
        {
            var userResult = await userRepository.IsUserExistsByIdAsync(userId, ct);
            if (!userResult)
                return Result<bool>.Failure("User does not exist", 404);

            var updateResult = await userRepository.BlockUserAsync(userId, ct);
            if (updateResult!)
                return Result<bool>.Failure("User unblocking was unsucessful", 500);
            return Result<bool>.Success(true);

            await logger.LogAsync(blockedById, UserLogAction.BlockUser, new
            {
                info = $"User " +
                       $"{userId} was blocked by {blockedById} at {user.BlockedAt}"
            }, userId, ct);
                
            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Failure("Blocking user was not successful", 400);
        }
    }

    public async Task<Result<bool>> DeletePublicationAsync(CreateViolationDto createDto, string adminId,
        CancellationToken ct)
    {
        try
        {
            Publication? publication = await publicationRepository.GetRawPublicationByIdAsync(createDto
                .ItemToRemoveId, ct);

            if (publication == null) return Result<bool>.Failure("Publication to delete was not found", 404);

            publication.IsDeleted = true;
            context.Publications.Update(publication);

            Violation violation = new Violation
            {
                ViolationText = createDto.RemovalReason,
                ViolatedAt = DateTime.UtcNow,
                ViolatedBy = publication.Author,
                ViolatedById = publication.AuthorId
            };

            bool result = await violationNotificationService
                .RegisterViolationAsync(publication.Author, violation, createDto.ViolationScoreIncrease, true);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Something went wrong during the process. Error: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteCommentAsync(CreateViolationDto createDto, CancellationToken ct)
    {
        try
        {
            Comment? comment = await commentRepository.GetRawCommentByIdAsync(createDto.ItemToRemoveId, ct);

            if (comment == null) return Result<bool>.Failure("Comment that was requested to be deleted was not found", 404);

            comment.IsDeleted = true;
            await commentRepository.DeleteCommentAsync(comment.Id, ct);

            Violation violation = new Violation
            {
                ViolationText = createDto.RemovalReason,
                ViolatedAt = DateTime.UtcNow,
                ViolatedById = comment.AuthorId
            };

            var result = await violationNotificationService
                .RegisterViolationAsync(violation, createDto.ViolationScoreIncrease, false, ct);

            return result;
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"During the processing there was exception: {ex.Message}", 500);
        }
    }


}