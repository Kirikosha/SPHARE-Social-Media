namespace API.Controllers;

using Application.Features.Users.Queries;
using Domain.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

public class AdminController : BaseApiController
{
    [HttpGet("get-users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await Mediator.Send(new GetUsersList.Query());
        return Ok(users); 
    }
}
