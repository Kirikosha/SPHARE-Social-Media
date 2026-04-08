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
    [HttpPut("edit")]
    public async Task<ActionResult<PublicUserDto>> UpdateProfile([FromForm]UpdatePublicUserDto updateUser)
    {
        return HandleResult(await Mediator.Send(new UpdatePublicUser.Command { UpdateUserModel = updateUser }));
    }

    [HttpGet("user-search")]
    public async Task<ActionResult<List<PublicUserDto>>> SearchForUserByUniqueNameIdentifier([FromQuery] string searchQuery)
    {
        return HandleResult(await Mediator.Send(new GetUsersBySearchString.Query { SearchString = searchQuery }));
    }
}
