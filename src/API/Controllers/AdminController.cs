using Application.Core.Pagination;
using Application.Helpers;

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
    public async Task<ActionResult<PagedList<AdminUserDto>>> GetUsers([FromQuery]PaginationParams paginationParams)
    {
        var users = await Mediator.Send(new GetUsersList.Query { PaginationParams = paginationParams});
        return Ok(users); 
    }

    [HttpPost("delete-comment")]
    public async Task<ActionResult> DeleteComment(CreateViolationDto violation)
    {
        return HandleResult(await Mediator.Send(new DeleteComment.Command() { Violation = violation }));
    }

    [HttpPost("delete-publication")]
    public async Task<ActionResult> DeletePublication(CreateViolationDto violation)
    {
        return HandleResult(await Mediator.Send(new DeletePublication.Command() { Violation = violation }));
    }

    [HttpPost("block-user")]
    public async Task<ActionResult> BlockUser([FromBody] string userId)
    {
        var adminId = User.GetUserId();
        return HandleResult(await Mediator.Send(new BlockUser.Command { UserToBlockId = userId, BlockedById = adminId}));
    }

    [HttpPost("unblock-user")]
    public async Task<ActionResult> UnblockUser([FromBody] string userId)
    {
        return HandleResult(await Mediator.Send(new UnblockUser.Command { UserId = userId }));
    }

    [HttpGet("violations/{id}")]
    public async Task<ActionResult<List<ViolationDto>>> GetViolationsByUserId(string id)
    {
         return HandleResult(await Mediator.Send(new GetViolationsByUserId.Query { UserId = id }));
    }
}
