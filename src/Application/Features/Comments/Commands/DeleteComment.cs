namespace Application.Features.Comments.Commands;

using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<bool>
    {
        public required int CommentId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            Comment? comment = await context.Comments.FindAsync(request.CommentId, cancellationToken);
            if (comment == null) return false;

            context.Comments.Remove(comment);
            return true;
        }
    }
}
