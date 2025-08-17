namespace API.Controllers;

using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Application.Helpers;
using Domain.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

public class PublicUserController : BaseApiController
{
    [HttpGet("by-uni")]
    public async Task<ActionResult<PublicUserDto>> GetPublicUserByUni(string uniqueNameIdentifier)
    {
        PublicUserDto? user = await Mediator.Send(
            new GetPublicUserByUniqueNameIdentifier.Query() { UniqueNameIdentifier = uniqueNameIdentifier });

        return user != null ? Ok(user) : BadRequest("User was not found");
    }

    [HttpGet("my-profile")]
    public async Task<ActionResult> GetMyProfile()
    {
        int id = User.GetUserId();

        PublicUserDto? user = await Mediator.Send(new GetPublicUserById.Query() { Id = id });
        
        return user != null ? Ok(user) : BadRequest("Something went wrong");
    }

    [HttpPut("edit")]
    public async Task<ActionResult<PublicUserDto>> UpdateProfile(UpdatePublicUserDto updateUser)
    {
        PublicUserDto? user = await Mediator.Send(new UpdatePublicUser.Command { UpdateUserModel = updateUser });
        
        return user != null ? Ok(user) : BadRequest("Something went wrong");
    }

    [HttpGet("user-search")]
    public async Task<ActionResult<List<PublicUserDto>>> SearchForUserByUniqueNameIdentifier([FromQuery] string searchQuery)
    {
        var users = await Mediator.Send(new GetUsersByUniqueNameIdentifier.Query { UniqueNameIdentifier = searchQuery });
        return Ok(users);
    }
}
