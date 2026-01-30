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

    [HttpGet("{parentId:int}/replies")]
    public async Task<ActionResult> GetReplies(int parentId)
    {
        return HandleResult(await Mediator.Send(new GetReplies.Query() { ParentId = parentId }));
    }

    [HttpGet("{id}/comment")]
    public async Task<ActionResult> GetComment(int id)
    {
        return HandleResult(await Mediator.Send(new GetComment.Query { Id = id }));
    }

    [HttpGet("{id:int}/comment-amount")]
    public async Task<ActionResult> GetCommentAmount(int id)
    {
        return HandleResult(await Mediator.Send(new GetCommentAmount.Query { Id = id }));
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
