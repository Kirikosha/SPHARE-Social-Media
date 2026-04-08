using Application.DTOs;
using Application.DTOs.MessagingDTOs;
using Application.Features.Messaging.Commands;
using Application.Features.Messaging.Queries;
using Application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ChatRqController : BaseApiController
{
    [HttpPost("start")]
    public async Task<ActionResult> StartChatting([FromBody] StartChatDto dto)
    {
        var userId = User.GetUserId();

        // returns if success: ChatDto
        return HandleResult(await Mediator.Send(new StartChat.Command { UserId = userId, ChatRequest = dto }));
    }

    [HttpGet("{chatId}/messages")]
    public async Task<ActionResult> GetMessages(string chatId, PageDto pageParameters)
    {
        var userId = User.GetUserId();
        // returns if success: List<MessageDto>
        return HandleResult(await Mediator.Send(new GetMessagesBatch.Query
            { UserId = userId, ChatId = chatId, Page = pageParameters }));
    }

    [HttpGet("{chatId}/open-chat")]
    public async Task<ActionResult> GetChat(string chatId)
    {
        return HandleResult(await Mediator.Send(new GetChat.Query { ChatId = chatId }));
    }
    
    [HttpGet("get-chats")]
    public async Task<ActionResult> GetChats()
    {
        var userId = User.GetUserId();
        // returns if success: List<ChatDto>
        return HandleResult(await Mediator.Send(new GetUserChats.Query { UserId = userId }));
    }
}