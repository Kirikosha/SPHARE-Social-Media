using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.UserDTOs;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Infrastructure.Services;

public class UserService(IUserRepository userRepository, ICloudinaryService cloudinaryService, IPhotoService 
        photoService, IMapper mapper, ApplicationDbContext context) : 
    IUserService
{
    private const int ViolationLimit = 20;
    public async Task<Result<PublicUserDto>> GetPublicUserByIdAsync(string id, CancellationToken ct)
    {
        var user = await userRepository.GetPublicUserByIdAsync(id, ct);
        return user == null
            ? Result<PublicUserDto>.Failure("User was not found", 404)
            : Result<PublicUserDto>.Success(user); 
    }

    public async Task<Result<PublicUserDto>> GetPublicUserByUniqueNameAsync(string uniqueName, CancellationToken ct)
    {
        var user = await userRepository.GetPublicUserByUniqueNameAsync(uniqueName, ct);
        return user == null
            ? Result<PublicUserDto>.Failure("User was not found", 404)
            : Result<PublicUserDto>.Success(user); 
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var user = await userRepository.GetUserByEmailAsync(email, ct);
        return user == null
            ? Result<User>.Failure("User was not found", 404)
            : Result<User>.Success(user); 
    }

    public async Task<Result<User>> GetUserByIdAsync(string id, CancellationToken ct)
    {
        var user = await userRepository.GetUserByIdAsync(id, ct);
        return user == null
            ? Result<User>.Failure("User was not found", 404)
            : Result<User>.Success(user); 
    }

    public async Task<Result<List<string>>> GetUserEmailsByIdsAsync(List<string> ids, CancellationToken ct)
    {
        try
        {
            var emails = await userRepository.GetUserEmailsByIds(ids, ct);
            return Result<List<string>>.Success(emails);
        }
        catch (Exception)
        {
            return Result<List<string>>.Failure("User emails gathering was unsuccessful", 400);
        }
    }

    public async Task<Result<List<PublicUserBriefDto>>> GetUsersBySearchString(string searchString, CancellationToken
            ct)
    {
        try
        {
            var users = await userRepository.SearchUsersBySearchStringAsync(searchString, ct);
            return Result<List<PublicUserBriefDto>>.Success(users);
        }
        catch (Exception)
        {
            return Result<List<PublicUserBriefDto>>.Failure("During search an error happened", 400);
        }
    }

    public async Task<Result<PagedList<AdminUserDto>>> GetUserListAsync(PaginationParams paginationParams, 
        CancellationToken ct)
    {
        try
        {
            var users = await userRepository.GetUserList(paginationParams, ct);
            return Result<PagedList<AdminUserDto>>.Success(users);
        }
        catch (Exception)
        {
            return Result<PagedList<AdminUserDto>>.Failure("Fetching admin user list was unsuccessful", 400);
        }
    }

    public async Task<Result<bool>> UpdateViolationScore(string userId, int violationScore, CancellationToken ct)
    {
        try
        {
            User? user = await userRepository.GetUserByIdAsync(userId, ct);
            if (user == null) return Result<bool>.Failure("User was not found", 404);

            user.ViolationScore += violationScore;

            if (user.ViolationScore >= ViolationLimit)
            {
                user.Blocked = true;
                user.BlockedAt = DateTime.UtcNow;
            }

            userRepository.UpdateUser(user, ct);
            return Result<bool>.Success(true);
        }
        catch (Exception)
        {
            return Result<bool>.Failure("User violation score was not updated", 400);
        }
    }

    // Update main info (username, unique name identifier)
    public async Task<OneOf<Unit, UniqueNamesOptions, Error>> UpdateUserMainInformation(UpdateUserMainInfoDto 
            updateModel, string userId, 
        CancellationToken ct)
    {
        var userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return UserErrors.UserNotFound();

        updateModel.Username = updateModel.Username.Trim();
        //TODO: perhaps add here some name verification mechanism for bad words

        updateModel.UniqueNameIdentifier = new string(updateModel.UniqueNameIdentifier.Where(c => !char.IsWhiteSpace(c)).ToArray());

        var uniqueNameIdentifier = await userRepository.BuildUniqueNameIdentifier(updateModel
            .UniqueNameIdentifier, ct);

        if (uniqueNameIdentifier != updateModel.UniqueNameIdentifier)
            return new UniqueNamesOptions() { UniqueNameOption = uniqueNameIdentifier };

        try
        {
            await userRepository.UpdateUserMainInfoAsync(updateModel, userId, ct);
            return Unit.Value;
        }
        catch
        {
            return UserErrors.UserMainInfoUpdateFailed();
        }
    }

    //TODO: Update additional info(pronouns, main profile description, interests, date of birth)
    public async Task<Result<Unit>> UpdateAdditionalInfo(UpdateUserAdditionalInfoDto updateModel, string userId, 
        CancellationToken ct)
    {
        var userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<Unit>.Failure(UserErrors.UserNotFound());
    }

    //TODO: Update image
    //TODO: Change email (with verification on new email and old email)
    //TODO: Change password (with verification via OTP code on email + comparison with the old password)
    public async Task<Result<PublicUserDto>> UpdateUser(UpdateUserMainInfoDto updateMainInfoDto, CancellationToken ct)
    {
        var user = await userRepository.GetUserForUpdateByIdAsync(updateMainInfoDto.Id, ct);

        if (user == null)
            return Result<PublicUserDto>.Failure("User was not found", 404);

        if (!string.IsNullOrWhiteSpace(updateMainInfoDto.Username))
            user.Username = updateMainInfoDto.Username;

        if (!string.IsNullOrWhiteSpace(updateMainInfoDto.UniqueNameIdentifier))
            user.UniqueNameIdentifier = updateMainInfoDto.UniqueNameIdentifier;

        ImageAction action;
        if (updateMainInfoDto.ProfileImage != null)
            action = ImageAction.New;
        else if (updateMainInfoDto.RemoveProfileImage && user.ProfileImage != null)
            action = ImageAction.Delete;
        else
            action = ImageAction.Keep;

        if (action == ImageAction.New)
        {
            var response = await cloudinaryService.AddPhotoAsync(updateMainInfoDto.ProfileImage!);
            if (response.Error != null)
                return Result<PublicUserDto>.Failure("Image was not uploaded", 500);

            Image image = new Image { PublicId = response.PublicId, ImageUrl = response.Url.AbsoluteUri };

            if (user.ProfileImage != null)
            {
                var result = await photoService.DeleteProfileImageAsync(user.ProfileImage.PublicId, ct);
                if (!result.IsSuccess)
                    return Result<PublicUserDto>.Failure(result.Error!, result.Code);
            }

            user.ProfileImage = image;
        }
        else if (action == ImageAction.Delete)
        {
            var result = await photoService.DeleteProfileImageAsync(user.ProfileImage!.PublicId, ct);
            if (!result.IsSuccess)
                return Result<PublicUserDto>.Failure(result.Error!, 500);
            user.ProfileImage = null;
        }


        if (updateMainInfoDto.UserProfileDetails != null)
        {
            if (user.ProfileDetails == null)
            {
                user.ProfileDetails = mapper.Map<UserProfileDetails>(updateMainInfoDto.UserProfileDetails);
                user.ProfileDetails.UserId = user.Id;
                context.ProfileDetails.Add(user.ProfileDetails);
            }
            else
            {
                mapper.Map(updateMainInfoDto.UserProfileDetails, user.ProfileDetails);
            }
        }
        else if (user.ProfileDetails != null && updateMainInfoDto.UserProfileDetails == null)
        {
            context.ProfileDetails.Remove(user.ProfileDetails);
            user.ProfileDetails = null;
        }

        if (updateMainInfoDto.Address != null)
        {
            if (user.Address == null)
            {
                user.Address = mapper.Map<Address>(updateMainInfoDto.Address);
                user.Address.UserId = user.Id;
                context.Addresses.Add(user.Address);
            }
            else
            {
                mapper.Map(updateMainInfoDto.Address, user.Address);
            }
        }
        else if (user.Address != null && updateMainInfoDto.Address == null)
        {
            context.Addresses.Remove(user.Address);
            user.Address = null;
        }

        return Result<PublicUserDto>.Success(mapper.Map<PublicUserDto>(user));
    }

    public async Task<Result<Unit>> UpdateUser(User user, CancellationToken ct)
    {
        try
        {
            userRepository.UpdateUser(user, ct);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit>.Failure("Update was unsuccessful", 500);
        }
    }
}