using Application.Core;
using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.MessagingDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messaging.Queries;

public class GetMessagesBatch
{
    public class Query : IRequest<Result<List<MessageDto>>>
    {
        public required string ChatId { get; set; }
        public required string UserId { get; set; }
        public PageDto Page { get; set; } = null!;
    }

    // TODO: Look at it once more because it is not absolutely complete I guess
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<MessageDto>>>
    {
        public async Task<Result<List<MessageDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var isParticipant = await context.ChatUsers
                .AnyAsync(cu => cu.ChatId == request.ChatId &&
                                cu.UserId == request.UserId, cancellationToken);

            if (!isParticipant)
            {
                return Result<List<MessageDto>>.Failure("You are not a part of this conversation!", 403);
            }

            var messages = await context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == request.ChatId)
                .OrderByDescending(m => m.SentAt)
                .Skip((request.Page.Page - 1) * request.Page.PageSize)
                .Take(request.Page.PageSize)
                .ToListAsync(cancellationToken);

            var messageDtoList = mapper.Map<List<MessageDto>>(messages);

            return Result<List<MessageDto>>.Success(messageDtoList);
        }
    }
}