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

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            Comment? comment = await context.Comments.Include(a => a.Replies).FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);
            if (comment == null) return Result<bool>.Failure("Comment was not found", 404);

            if (comment.Replies.Count == 0)
            {
                context.Comments.Remove(comment);
                return Result<bool>.Success(true);
            }

            comment.IsDeleted = true;

            context.Update(comment);
            return Result<bool>.Success(true);
        }
    }
}
