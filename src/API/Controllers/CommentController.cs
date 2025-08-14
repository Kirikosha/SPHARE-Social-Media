namespace API.Controllers;

using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Helpers;
using Domain.DTOs.CommentDTOs;
using Microsoft.AspNetCore.Mvc;

public class CommentController : BaseApiController
{
    [HttpGet("{publicationId:int}")]
    public async Task<ActionResult> GetComments(int publicationId)
    {
        List<CommentDto> comments = await Mediator.Send(new GetCommentsByPublicationId.Query() { PublicationId = publicationId });
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto commentModel)
    {
        var userId = User.GetUserId();

        var comment = await Mediator.Send(new CreateComment.Command() { Comment = commentModel, UserId = userId });
        if (comment == null) return BadRequest("Something went wrong during execution");
        return Ok(comment);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        bool result = await Mediator.Send(new DeleteComment.Command() { CommentId = id });
        return result ? Ok() : BadRequest("Comment was not deleted due to an error");
    }
}
