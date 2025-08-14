namespace API.Controllers;

using Application.Features.AdminFeatures.Commands;
using Application.Features.Users.Queries;
using Domain.DTOs.UserDTOs;
using Domain.DTOs.ViolationDTOs;
using Microsoft.AspNetCore.Mvc;

public class AdminController : BaseApiController
{
    [HttpGet("get-users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await Mediator.Send(new GetUsersList.Query());
        return Ok(users); 
    }

    [HttpPost("delete-comment")]
    public async Task<ActionResult> DeleteComment(CreateViolationDto violation)
    {
        bool result = await Mediator.Send(new DeleteComment.Command() { Violation = violation });
        return result ? Ok() : BadRequest("Something went wrong");
    }
}
