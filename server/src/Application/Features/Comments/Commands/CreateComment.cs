namespace Application.Features.Comments.Commands;
using DTOs.CommentDTOs;
using Application.Interfaces.Services;
using Core;
using System.Threading;
using System.Threading.Tasks;

public class CreateComment
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public required string UserId { get; set; }
        public required CreateCommentDto Comment { get; set; }
    }

    public class Handler(ICommentService commentService) : IRequestHandler<Command, Result<CommentDto>>
    {

        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await commentService.CreateCommentAsync(request.UserId, request.Comment, cancellationToken);
        }
    }
}
