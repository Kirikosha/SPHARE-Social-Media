using Application.Features.Account.Commands;
using Domain.DTOs.AccountDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class AccountController : BaseApiController
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginModel)
    {
        return HandleResult(await Mediator.Send(
            new Login.Command { LoginModel = loginModel }));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerModel)
    {
        return HandleResult(await Mediator.Send(
            new Register.Command { RegisterModel = registerModel }));
    }
    
    [HttpPost("refresh")]                          // ← add this
    public async Task<ActionResult> Refresh([FromBody] string refreshToken)
    {
        return HandleResult(await Mediator.Send(
            new RefreshToken.Command { Token = refreshToken }));
    }
}