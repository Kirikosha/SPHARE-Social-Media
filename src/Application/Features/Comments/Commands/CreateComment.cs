namespace Application.Features.Comments.Commands;

using AutoMapper;
using Domain.DTOs.CommentDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class CreateComment
{
    public class Command : IRequest<CommentDto>
    {
        public required int UserId { get; set; }
        public required CreateCommentDto Comment { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Command, CommentDto>
    {
        public async Task<CommentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users.Include(a => a.ProfileImage).FirstOrDefaultAsync(a => a.Id == request.UserId);
            if (user == null) throw new Exception("User was not found");

            Publication? publication = await context.Publications.FindAsync(request.Comment.PublicationId);
            if (publication == null) throw new Exception("Publication you are writing comment to no longer exist");

            Comment comment = new Comment
            {
                Author = user,
                Publication = publication,
                Content = request.Comment.Content,
                CreationDate = DateTime.UtcNow
            };

            context.Comments.Add(comment);

            return mapper.Map<CommentDto>(comment);
        }
    }
}
