namespace Application.Features.Comments.Commands;

using Application.Core;
using Infrastructure;
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
            Comment? comment = await context.Comments.FindAsync(request.CommentId, cancellationToken);
            if (comment == null) return Result<bool>.Failure("Comment was not found", 404);

            context.Comments.Remove(comment);
            return Result<bool>.Success(true);
        }
    }
}
