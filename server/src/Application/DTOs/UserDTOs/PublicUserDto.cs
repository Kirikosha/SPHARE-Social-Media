using Application.DTOs.DetailedUserInfoDTOs;

namespace Application.DTOs.UserDTOs;
public class PublicUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string JoinedAt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool Blocked { get; set; }
    public UserProfileDetailsDto? UserProfileDetails { get; set; }
    public AddressDto? Address { get; set; }
}
