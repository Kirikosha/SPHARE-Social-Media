using Application.Repositories.UserActivityLogRepository;
using Application.Services.UserActionLogger;
using Domain.DTOs;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Features.Comments.Commands;

using Application.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required int CommentId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IUserActionLogger<DeleteComment> logger) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            Comment? comment = await context.Comments.Include(a => a.Replies).FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);
            if (comment == null) return Result<bool>.Failure("Comment was not found", 404);

            comment.IsDeleted = true;

            context.Update(comment);

            await logger.LogAsync(comment.AuthorId, UserLogAction.DeleteComment, new
            {
                info = $"Comment {comment.Id} was " +
                       $"deleted by user {comment.AuthorId}"
            }, cancellationToken);
            
            return Result<bool>.Success(true);
        }
        
    }
}
