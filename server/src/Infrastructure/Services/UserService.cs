using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Infrastructure.Services;

public class UserService(IUserRepository userRepository, IPhotoService 
        photoService, IMapper mapper) : IUserService
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
    public async Task<OneOf<Unit, UniqueNamesOptions, Error>> UpdateUserMainInformationAsync(UpdateUserMainInfoDto 
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

    // Update additional info(pronouns, main profile description, interests, date of birth)
    public async Task<Result<UserProfileDetailsDto>> UpdateUserAdditionalInfoAsync(UpdateUserAdditionalInfoDto updateModel, 
        string userId, 
        CancellationToken ct)
    {
        if (!await userRepository.IsUserExistsByIdAsync(userId, ct))
        {
            return Result<UserProfileDetailsDto>.Failure(UserErrors.UserNotFound());
        }

        var profile = await userRepository.GetUserProfileDetailsByUserIdAsync(userId, ct) 
                      ?? new UserProfileDetails { UserId = userId };

        profile.Pronouns = updateModel.Pronouns;
        profile.MainProfileDescription = updateModel.MainProfileDescription;
        profile.DateOfBirth = updateModel.DateOfBirth;
        profile.Interests = updateModel.Interests;

        // TODO: it returns UserProfileDetails so think about using it
        var details = await userRepository.SetUserProfileDetailsAsync(profile, ct);

        return mapper.Map<UserProfileDetailsDto>(details);
    }

    // TODO: Update user address
    public async Task<Result<AddressDto>> SetUserAddressAsync(UpdateUserAddressDto updateModel, string userId,
        CancellationToken ct)
    {
        var userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<AddressDto>.Failure(UserErrors.UserNotFound());

        var userAddress = await userRepository.GetUserAddressByIdAsync(userId, ct)
                          ?? new Address { UserId = userId };

        userAddress.City = updateModel.City;
        userAddress.Country = updateModel.Country;
        
        // TODO: it returns Address so think about using it
        var address = await userRepository.UpdateUserAddressAsync(userAddress, ct);
        return mapper.Map<AddressDto>(address);
    }

    // Update image
    public async Task<Result<Unit>> UpdateProfileImageAsync(UpdateUserProfileImageDto updateModel, string userId, 
        CancellationToken ct)
    {
        var userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<Unit>.Failure(UserErrors.UserNotFound());

        var imageExists = await photoService.IsProfileImageExists(userId, ct);
        Result<Unit> result = await photoService.UploadUserProfilePicture(updateModel.ProfileImage, userId, ct);

        if (result.IsSuccess)
        {
            return Unit.Value;
        }

        return Result<Unit>.Failure(result.Error!, result.Code);

    }

    // Delete image
    public async Task<Result<Unit>> DeleteProfileImageAsync(string userId, CancellationToken ct)
    {
        var userExists = await userRepository.IsUserExistsByIdAsync(userId, ct);
        if (!userExists)
            return Result<Unit>.Failure(UserErrors.UserNotFound()); 
        var deletionResult = await photoService.DeleteProfileImageAsync(userId, ct);
        if (deletionResult.IsSuccess)
            return Result<Unit>.Success(Unit.Value);

        return Result<Unit>.Failure(deletionResult.Error!, deletionResult.Code);
    }

    //TODO: Change email (with verification on new email and old email)
    //TODO: Change password (with verification via OTP code on email + comparison with the old password)

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
    
    public async Task<string?> GetUserEmailByIdAsync(string userId, CancellationToken ct)
        => await userRepository.GetUserEmailByIdAsync(userId, ct);
}