using Application.Core;
using Application.Core.Pagination;
using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<PublicUserDto>> GetPublicUserByIdAsync(string id, CancellationToken ct);
    Task<Result<PublicUserDto>> GetPublicUserByUniqueNameAsync(string uniqueName, CancellationToken ct);
    Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<Result<User>> GetUserByIdAsync(string id, CancellationToken ct);
    Task<Result<List<string>>> GetUserEmailsByIdsAsync(List<string> ids, CancellationToken ct);
    Task<Result<List<PublicUserBriefDto>>> GetUsersBySearchString(string searchString, CancellationToken ct);
    Task<Result<PagedList<AdminUserDto>>> GetUserListAsync(PaginationParams paginationParams, CancellationToken ct);
    Task<Result<bool>> UpdateViolationScore(string userId, int violationScore, CancellationToken ct);
    Task<OneOf<Unit, UniqueNamesOptions, Error>> UpdateUserMainInformationAsync(UpdateUserMainInfoDto updateModel, string userId,
        CancellationToken ct);
    Task<Result<UserProfileDetailsDto>> UpdateUserAdditionalInfoAsync(UpdateUserAdditionalInfoDto updateModel,
        string userId,
        CancellationToken ct);
    Task<Result<AddressDto>> SetUserAddressAsync(UpdateUserAddressDto updateModel, string userId, CancellationToken ct);
    Task<Result<Unit>> UpdateProfileImageAsync(UpdateUserProfileImageDto updateModel, string userId,
        CancellationToken ct);
    Task<Result<Unit>> DeleteProfileImageAsync(string userId, CancellationToken ct);
    Task<string?> GetUserEmailByIdAsync(string userId, CancellationToken ct);
}