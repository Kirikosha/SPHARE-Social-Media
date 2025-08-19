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

        bool result = await Mediator.Send(new CreatePublication.Command { CreatorId = userId, Publication = publication });
        return result ? Ok() : BadRequest();
    }

    [HttpPut("update-publication")]
    public async Task<ActionResult<PublicationDto>> UpdatePublication(UpdatePublicationDto publication)
    {
        var userId = User.GetUserId();

        PublicationDto? updatedPublication = await Mediator
            .Send(new UpdatePublication.Command { Publication = publication, UserId = userId });

        return Ok(updatedPublication);
    }

    [HttpGet("publication-of-{uniqueNameIdentifier}")]
    public async Task<ActionResult<List<PublicationDto>>> GetPublicationsOfAuthor(string uniqueNameIdentifier)
    {
        var userId = User.GetUserId();

        var publications = await Mediator.Send(
            new GetPublicationsByUniqueNameIdentifier.Query
            {
                UniqueNameIdentifier = uniqueNameIdentifier,
                UserId = userId
            }
        );

        return Ok(publications);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublicationDto>> GetPublicationById(int id)
    {
        int userId = User.GetUserId();

        var publication = await Mediator.Send(new GetPublicationById.Query
        {
            PublicationId = id,
            UserId = userId
        });

        return Ok(publication);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePublication(int id)
    {
        await Mediator.Send(new DeletePublication.Command { Id = id });
        return Ok();
    }

    [HttpGet("like/{id:int}")]
    public async Task<ActionResult<LikeDto>> LikePublication(int id)
    {
        var userId = User.GetUserId();

        var likeProcessed = await Mediator.Send(new LikePublication.Command
        {
            PublicationId = id,
            UserId = userId
        });

        return Ok(likeProcessed);
    }
}
