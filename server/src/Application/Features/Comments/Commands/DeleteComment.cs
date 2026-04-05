using Application.Services.UserActionLogger;
using Domain.Enums;

namespace Application.Features.Comments.Commands;

using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required string CommentId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IUserActionLogger<DeleteComment> logger) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var comment = await context.Comments
                .Where(c => c.Id == request.CommentId)
                .Select(c => new { c.Id, c.AuthorId })
                .FirstOrDefaultAsync(cancellationToken);

            if (comment == null) 
                return Result<bool>.Failure("Comment was not found", 404);

            await context.Comments
                .Where(c => c.Id == request.CommentId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true), cancellationToken);

            await logger.LogAsync(comment.AuthorId, UserLogAction.DeleteComment, new
            {
                info = $"Comment {comment.Id} was " +
                       $"deleted by user {comment.AuthorId}"
            }, comment.Id, cancellationToken);
            
            return Result<bool>.Success(true);
        }
        
    }
}
