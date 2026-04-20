using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.UserDTOs;

public class UpdateUserProfileImageDto
{
    public IFormFile ProfileImage { get; set; } = null!;
}