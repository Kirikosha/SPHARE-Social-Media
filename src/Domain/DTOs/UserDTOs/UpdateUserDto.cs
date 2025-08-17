namespace Domain.DTOs.UserDTOs;

using Domain.Enums;
using Microsoft.AspNetCore.Http;

public class UpdatePublicUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string JoinedAt { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public ImageAction Action { get; set; } = ImageAction.Keep;
}
