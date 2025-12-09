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
        return HandleResult(await Mediator.Send(new GetCommentsByPublicationId.Query() { PublicationId = publicationId }));
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto commentModel)
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator.Send(new CreateComment.Command() { Comment = commentModel, UserId = userId }));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        return HandleResult(await Mediator.Send(new DeleteComment.Command() { CommentId = id }));
    }
}
