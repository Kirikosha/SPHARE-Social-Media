namespace Application.Features.Comments.Commands;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class DeleteComment
{
    public class Command : IRequest<Result<bool>>
    {
        public required string CommentId { get; set; }
    }

    public class Handler(ICommentService commentService) : 
        IRequestHandler<Command, 
        Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await commentService.DeleteCommentAsync(request.CommentId, cancellationToken);
        }
        
    }
}
