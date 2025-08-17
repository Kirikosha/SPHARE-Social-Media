namespace API.Controllers;

using Application.Features.Publications.Commands;
using Application.Helpers;
using Domain.DTOs.PublicationDTOs;
using Microsoft.AspNetCore.Mvc;

public class PublicationController : BaseApiController
{
    [HttpPost("create-publication")]
    public async Task<ActionResult> CreatePublication(CreatePublicationDto publication)
    {
        int userId = User.GetUserId();

        bool result = await Mediator.Send(new CreatePublication.Command { CreatorId = userId, Publication = publication });
        return result ? Ok() : BadRequest();
    }
}
