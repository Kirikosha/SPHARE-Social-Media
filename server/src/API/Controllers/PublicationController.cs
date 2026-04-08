using Application.DTOs.LikeDTOs;
using Application.DTOs.PublicationDTOs;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

using Application.Features.Likes.Commands;
using Application.Features.Publications.Commands;
using Application.Features.Publications.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class PublicationController : BaseApiController
{
    [Authorize]
    [HttpPost("create-publication")]
    public async Task<ActionResult> CreatePublication(CreatePublicationDto publication)
    {
        string userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new CreatePublication.Command { CreatorId = userId, Publication = publication }));
    }

    [Authorize]
    [HttpPut("update-publication")]
    public async Task<ActionResult<PublicationDto>> UpdatePublication(UpdatePublicationDto publication)
    {
        var userId = User.GetUserId();

         return HandleResult(await Mediator
            .Send(new UpdatePublication.Command { Publication = publication, UserId = userId }));

    }

    [Authorize]
    [HttpGet("publication-of-{uniqueNameIdentifier}")]
    public async Task<ActionResult<List<PublicationDto>>> GetPublicationsOfAuthor(string uniqueNameIdentifier)
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(
            new GetPublicationsByUniqueNameIdentifier.Query
            {
                UniqueNameIdentifier = uniqueNameIdentifier,
                UserId = userId
            }
        ));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<PublicationDto>> GetPublicationById(string id)
    {
        string userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetPublicationById.Query
        {
            PublicationId = id,
            UserId = userId
        }));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePublication(string id)
    {
        return HandleResult(await Mediator.Send(new DeletePublication.Command { Id = id }));
    }

    [Authorize]
    [HttpGet("like/{id}")]
    public async Task<ActionResult<LikeDto>> LikePublication(string id)
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new LikePublication.Command
        {
            PublicationId = id,
            UserId = userId
        }));
    }

    // TODO: replace with filtration + pagination
    /*
    [HttpGet("planned-publications")]
    public async Task<ActionResult> GetPlannedPublications()
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator.Send(new GetPlannedPublications.Query { UserId = userId }));
    }
    */
    [Authorize]
    [HttpGet("publication-calendar")]
    public async Task<ActionResult> GetPublicationModelForCalendar()
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetPublicationsCalendar.Query { UserId = userId }));
    }

    [Authorize]
    [HttpGet("publication/view-update/{id}")]
    public async Task<ActionResult> PublicationViewAdded(string id)
    {
        return HandleResult(await Mediator.Send(new UpdatePublicationViews.Command { PublicationId = id }));
    }
    
}
