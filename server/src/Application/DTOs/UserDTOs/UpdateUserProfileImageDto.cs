using System.Text.Json.Serialization;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.UserDTOs;

public class UpdateUserProfileImageDto
{
    [JsonPropertyName("profileImage")]
    public IFormFile ProfileImage { get; set; } = null!;
}