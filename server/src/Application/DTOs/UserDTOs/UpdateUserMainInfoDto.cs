using System.Text.Json.Serialization;

namespace Application.DTOs.UserDTOs;

public class UpdateUserMainInfoDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("uniqueNameIdentifier")]
    public string UniqueNameIdentifier { get; set; } = string.Empty;
}
