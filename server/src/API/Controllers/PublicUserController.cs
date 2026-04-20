using Application.DTOs.DetailedUserInfoDTOs;
using Application.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class PublicUserController : BaseApiController
{
    [HttpGet("by-uni")]
    public async Task<ActionResult<PublicUserDto>> GetPublicUserByUni([FromQuery]string uNi)
    {
        return HandleResult(await Mediator.Send(
            new GetPublicUserByUniqueNameIdentifier.Query() { UniqueNameIdentifier = uNi }));
    }

    [Authorize]
    [HttpGet("my-profile")]
    public async Task<ActionResult> GetMyProfile()
    {
        string id = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetPublicUserById.Query() { Id = id }));
    }

    [Authorize]
    [HttpPut("update-main-info")]
    public async Task<ActionResult<PublicUserDto>> UpdateMainInfo([FromForm] UpdateUserMainInfoDto
        updateUserMainInfoDto, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await Mediator.Send(new UpdateUserMainInfo.Command()
        {
            MainInfo = updateUserMainInfoDto,
            UserId = userId
        }, ct);

        return result.Match(
            user => Ok(user),
            options => Conflict(new { Options = options }),
            error => Problem(error.Message, statusCode: error.Code));
    }

    [Authorize]
    [HttpPut("update-additional-info")]
    public async Task<ActionResult<UserProfileDetailsDto>> UpdateAdditionalInfo(
        [FromForm] UpdateUserAdditionalInfoDto updateUserAdditionalInfoDto, CancellationToken ct )
    {
        var userId = User.GetUserId();
        var result = await Mediator.Send(new UpdateUserAdditionalInfo.Command
        {
            UserId = userId,
            UpdateModel = updateUserAdditionalInfoDto
        }, ct);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("update-address")]
    public async Task<ActionResult<AddressDto>> UpdateAddress([FromForm] UpdateUserAddressDto updateUserAddressDto,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await Mediator.Send(new UpdateUserAddress.Command
        {
            UpdateModel = updateUserAddressDto,
            UserId = userId
        }, ct);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("update-profile-image")]
    public async Task<ActionResult> UpdateProfileImage([FromForm] UpdateUserProfileImageDto updateUserProfileImageDto,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await Mediator.Send(new UpdateUserProfileImage.Command
        {
            UserId = userId,
            UpdateModel = updateUserProfileImageDto
        }, ct);

        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("delete-user-image")]
    public async Task<ActionResult> DeleteProfileImage(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await Mediator.Send(new DeleteUserProfileImage.Command
        {
            UserId = userId
        }, ct);

        return HandleResult(result);
    }

    [HttpGet("user-search")]
    public async Task<ActionResult<List<PublicUserDto>>> SearchForUserByUniqueNameIdentifier([FromQuery] string searchQuery)
    {
        return HandleResult(await Mediator.Send(new GetUsersBySearchString.Query { SearchString = searchQuery }));
    }
}
