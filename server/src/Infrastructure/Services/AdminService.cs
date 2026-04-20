using Application.Core;
using Application.DTOs.ViolationDTOs;
using Application.Interfaces.Logger;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public class AdminService(IUserService userService, IViolationNotificationService violationNotificationService,
    IUserActionLogger<AdminService> logger) 
    : IAdminService
{
    public async Task<Result<bool>> UnblockUserAsync(string userId, CancellationToken ct)
    {
        try
        {
            var userResult = await userService.GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess)
                return Result<bool>.Failure(userResult.Error!, userResult.Code);
            
            userResult.Value!.Blocked = false;
            userResult.Value.BlockedAt = null;
            userResult.Value.ViolationScore = 0;

            var updateResult = await userService.UpdateUserMainInfo(userResult.Value, ct);
            if (updateResult!.IsSuccess)
                return Result<bool>.Failure(updateResult.Error!, updateResult.Code);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Something went wrong during the process. Error: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeletePublicationAsync(CreateViolationDto createDto, CancellationToken ct)
    {
        try
        {
            Publication? publication = await context.Publications.Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == createDto.ItemToRemoveId, ct);

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
        try {
            Comment? comment = await context.Comments.Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == createDto.ItemToRemoveId, ct);

            if (comment == null) return Result<bool>.Failure("Comment that was requested to be deleted was not found", 404);

            comment.IsDeleted = true;
            context.Comments.Update(comment);

            Violation violation = new Violation
            {
                ViolationText = createDto.RemovalReason,
                ViolatedAt = DateTime.UtcNow,
                ViolatedBy = comment.Author,
                ViolatedById = comment.AuthorId
            };

            bool result = await violationNotificationService
                .RegisterViolationAsync(comment.Author, violation, createDto.ViolationScoreIncrease, false);

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"During the processing there was exception: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> BlockUserAsync(string userToBlockId, string blockedById, CancellationToken ct)
    {
        try
        {
            User? user = await context.Users.FindAsync(userToBlockId, ct);
            if (user == null) throw new Exception("User was not found");

            user.Blocked = true;
            user.BlockedAt = DateTime.UtcNow;
            context.Users.Update(user);

            await logger.LogAsync(blockedById, UserLogAction.BlockUser, new
            {
                info = $"User " +
                       $"{userToBlockId} was blocked by {blockedById} at {user.BlockedAt}"
            }, userToBlockId, ct);
                
            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Failure("Blocking user was not successful", 400);
        }
    }
}