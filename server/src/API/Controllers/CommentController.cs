using Application.Core.Pagination;
using Application.DTOs.CommentDTOs;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Mvc;

public class CommentController : BaseApiController
{
    [HttpGet("{publicationId}")]
    public async Task<ActionResult> GetComments(string publicationId, [FromQuery] PaginationParams paginationParams)
    {
        return HandleResult(await Mediator.Send(new GetCommentsByPublicationId.Query() { PublicationId = 
            publicationId, Pagination = paginationParams}));
    }

    [HttpGet("{parentId}/replies")]
    public async Task<ActionResult> GetReplies(string parentId, [FromQuery] PaginationParams paginationParams)
    {
        return HandleResult(await Mediator
            .Send(new GetReplies.Query() { ParentId = parentId, Pagination = paginationParams}));
    }

    [HttpGet("{id}/comment")]
    public async Task<ActionResult> GetComment(string id)
    {
        return HandleResult(await Mediator.Send(new GetComment.Query { Id = id }));
    }

    [HttpGet("{id}/comment-amount")]
    public async Task<ActionResult> GetCommentAmount(string id)
    {
        return HandleResult(await Mediator.Send(new GetCommentAmount.Query { PublicationId = id }));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto commentModel)
    {
        var userId = User.GetUserId();

        return HandleResult(await Mediator
            .Send(new CreateComment.Command() { Comment = commentModel, UserId = userId }));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(string id)
    {
        var userId = User.GetUserId();
        return HandleResult(await Mediator.Send(new DeleteComment.Command()
        {
            CommentId = id,
            UserId = userId
        }));
    }

}
