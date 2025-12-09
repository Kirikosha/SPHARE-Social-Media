using Application.Features.Account.Commands;
using Domain.DTOs.AccountDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
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
    }
}
