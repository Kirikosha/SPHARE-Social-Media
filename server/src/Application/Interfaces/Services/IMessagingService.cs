using Application.Core;
using Application.DTOs.MessagingDTOs;

namespace Application.Interfaces.Services;

public interface IMessagingService
{
    Task<Result<ChatWithMessagesDto>> GetChatAsync(string chatId, CancellationToken ct);
    Task<Result<List<ChatDto>>> GetUserChatsAsync(string userId, CancellationToken ct);

    Task<Result<List<MessageDto>>> GetMessagesBatchAsync(string chatId, string userId, int page, int pageSize,
        CancellationToken ct);

    Task<Result<ChatDto>> StartChatAsync(StartChatDto startDto, string userId, CancellationToken ct);

    Task<Result<ReceiveMessageDto>> SaveMessageAsync(string? chatId, string receiverId, string senderId, string message,
        CancellationToken ct);

    Task<Result<Chat>> CreateChatAsync(string userId1, string userId2, CancellationToken ct);
}