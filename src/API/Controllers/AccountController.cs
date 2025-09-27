using Domain.DTOs.AccountDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginModel)
        {

        }
    }
}
