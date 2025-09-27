namespace Application.Features.Comments.Commands;

using Application.Core;
using AutoMapper;
using Domain.DTOs.CommentDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class CreateComment
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public required int UserId { get; set; }
        public required CreateCommentDto Comment { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Command, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.Include(a => a.ProfileImage).FirstOrDefaultAsync(a => a.Id == request.UserId);
            if (user == null) return Result<CommentDto>.Failure("User was not found", 400);

            Publication? publication = await context.Publications.FindAsync(request.Comment.PublicationId);
            if (publication == null) return Result<CommentDto>.Failure("Publication you are trying to delete no longer exists", 400);

            Comment comment = new Comment
            {
                Author = user,
                Publication = publication,
                Content = request.Comment.Content,
                CreationDate = DateTime.UtcNow
            };

            context.Comments.Add(comment);

            return Result<CommentDto>.Success(mapper.Map<CommentDto>(comment));
        }
    }
}
