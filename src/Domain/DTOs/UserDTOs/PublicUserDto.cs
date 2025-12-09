using Domain.DTOs.DetailedUserInfoDTOs;

namespace Domain.DTOs.UserDTOs;
public class PublicUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string JoinedAt { get; set; } = string.Empty;
    public ImageDto? ProfileImage { get; set; }
    public bool Blocked { get; set; }
    public UserProfileDetailsDto? UserProfileDetails { get; set; }
    public AddressDto? Address { get; set; }
}
