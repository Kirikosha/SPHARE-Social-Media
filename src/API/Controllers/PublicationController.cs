using System.Reflection.Metadata;

namespace API.Controllers;

using Application.Features.Likes.Commands;
using Application.Features.Publications.Commands;
using Application.Features.Publications.Queries;
using Application.Helpers;
using Domain.DTOs.LikeDTOs;
using Domain.DTOs.PublicationDTOs;
using Microsoft.AspNetCore.Mvc;

public class PublicationController : BaseApiController
{
    [HttpPost("create-publication")]
    public async Task<ActionResult> CreatePublication(CreatePublicationDto publication)
    {
        int userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new CreatePublication.Command { CreatorId = userId, Publication = publication }));
    }

    [HttpPut("update-publication")]
    public async Task<ActionResult<PublicationDto>> UpdatePublication(UpdatePublicationDto publication)
    {
        var userId = User.GetUserId();

         return HandleResult(await Mediator
            .Send(new UpdatePublication.Command { Publication = publication, UserId = userId }));

    }

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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublicationDto>> GetPublicationById(int id)
    {
        int userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetPublicationById.Query
        {
            PublicationId = id,
            UserId = userId
        }));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePublication(int id)
    {
        return HandleResult(await Mediator.Send(new DeletePublication.Command { Id = id }));
    }

    [HttpGet("like/{id:int}")]
    public async Task<ActionResult<LikeDto>> LikePublication(int id)
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new LikePublication.Command
        {
            PublicationId = id,
            UserId = userId
        }));
    }

    // TODO: Think about setting sort of a query for date like a month or something like that instead of getting all
    // planned publications
    [HttpGet("planned-publications")]
    public async Task<ActionResult> GetPlannedPublications()
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator.Send(new GetPlannedPublications.Query { UserId = userId }));
    }

    [HttpGet("publication-calendar")]
    public async Task<ActionResult> GetPublicationModelForCalendar()
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new GetPublicationsCalendar.Query { UserId = userId }));
    }
}
