namespace Domain.DTOs.UserDTOs;

using Domain.DTOs.DetailedUserInfoDTOs;
using Domain.Enums;
using Domain.ModelBinder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class UpdatePublicUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UniqueNameIdentifier { get; set; } = string.Empty;
    public string JoinedAt { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public ImageAction Action { get; set; } = ImageAction.Keep;
    [ModelBinder(BinderType = typeof(JsonModelBinder))] 
    public SetUserProfileDetailsDto? UserProfileDetails { get; set; }
    [ModelBinder(BinderType = typeof(JsonModelBinder))] 
    public SetAddressDto? Address { get; set; }
}
