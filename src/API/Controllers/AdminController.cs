namespace API.Controllers;

using Application.Features.AdminFeatures.Commands;
using Application.Features.Users.Queries;
using Application.Features.Violations.Queries;
using Domain.DTOs.UserDTOs;
using Domain.DTOs.ViolationDTOs;
using Microsoft.AspNetCore.Mvc;

public class AdminController : BaseApiController
{
    [HttpGet("get-users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
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

    [HttpPost("delete-publication")]
    public async Task<ActionResult> DeletePublication(CreateViolationDto violation)
    {
        bool result = await Mediator.Send(new DeletePublication.Command() { Violation = violation });
        return result ? Ok() : BadRequest("Something went wrong");
    }

    [HttpPost("block-user")]
    public async Task<ActionResult> BlockUser([FromBody] int userId)
    {
        bool result = await Mediator.Send(new BlockUser.Command { UserId = userId });
        return result ? Ok() : BadRequest("Something went wrong");
    }

    [HttpPost("unblock-user")]
    public async Task<ActionResult> UnblockUser([FromBody] int userId)
    {
        bool result = await Mediator.Send(new UnblockUser.Command { UserId = userId });
        return result ? Ok() : BadRequest("Something went wrong");
    }

    [HttpGet("violations/{id:int}")]
    public async Task<ActionResult<List<ViolationDto>>> GetViolationsByUserId(int id)
    {
        List<ViolationDto> violations = await Mediator.Send(new GetViolationsByUserId.Query { UserId = id });
        return Ok(violations);
    }
}
