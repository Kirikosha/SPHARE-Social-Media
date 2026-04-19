namespace API.Controllers;

using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator
    {
        get
        {
            if (_mediator == null)
            {
                _mediator = HttpContext.RequestServices.GetService<IMediator>();
            }
            return _mediator!;
        }
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return Problem(
            detail: result.Error,
            statusCode: result.Code
        );
    }
}
