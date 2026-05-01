using Application.Core.Pagination;
using Application.DTOs.CommentDTOs;
using Application.DTOs.UserDTOs;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailWithRefreshTokensAsync(string email, CancellationToken ct);
    Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken ct);
    Task<PublicUserDto?> GetPublicUserByIdAsync(string id, CancellationToken ct);
    Task<PublicUserDto?> GetPublicUserByUniqueNameAsync(string uniqueName, CancellationToken ct);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetUserByIdAsync(string id, CancellationToken ct);
    Task<User?> GetUserByRefreshTokenAsync(string token, CancellationToken ct);
    Task<List<string>> GetUserEmailsByIds(List<string> ids, CancellationToken ct);
    Task<List<PublicUserBriefDto>> SearchUsersBySearchStringAsync(string searchString, CancellationToken ct);
    Task<PagedList<AdminUserDto>> GetUserList(PaginationParams paginationParams, CancellationToken ct);
    void UpdateUser(User user, CancellationToken ct);
    Task UpdateUserMainInfoAsync(UpdateUserMainInfoDto mainInfo, string userId, CancellationToken ct);
    Task<UserProfileDetails> SetUserProfileDetailsAsync(UserProfileDetails profileDetails, CancellationToken ct);
    Task<Address> UpdateUserAddressAsync(Address address, CancellationToken ct);
    Task<User?> GetUserForUpdateByIdAsync(string id, CancellationToken ct);
    Task<bool> IsUserExistsByEmailAsync(string email, CancellationToken ct);
    Task<bool> IsUserExistsByIdAsync(string id, CancellationToken ct);
    Task<bool> CreateUserAsync(User user, CancellationToken ct);
    Task<string> BuildUniqueNameIdentifier(string username, CancellationToken ct);

    Task<UserProfileDetails?> GetUserProfileDetailsByUserIdAsync(string userId, CancellationToken ct);
    Task<Address?> GetUserAddressByIdAsync(string userId, CancellationToken ct);
    Task<bool> SetProfileImageId(string userId, string profileImageId, CancellationToken ct);

    Task<UserInfoProjection?> GetUserInfoAsync(string userId, string publicationId,
        CancellationToken ct);
    
    // TODO: perhaps think about replacing boolean with an enum to show the state of users other than ban or not
    Task<bool> BlockUserAsync(string userId, CancellationToken ct);
    Task<bool> UnBlockUserAsync(string userId, CancellationToken ct);

    Task<string?> GetUserEmailByIdAsync(string userId, CancellationToken ct);
}