namespace Application.Features.Likes.Commands;

using Domain.DTOs.LikeDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class LikePublication
{
    public class Command : IRequest<LikeDto>
    {
        public required int PublicationId { get; set; }
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, LikeDto>
    {
        public async Task<LikeDto> Handle(Command request, CancellationToken cancellationToken)
        {
            bool isLikedByUser = false;

            var postExists = await context.Publications.AnyAsync(a => a.Id == request.PublicationId);
            var userExists = await context.Users.AnyAsync(a => a.Id == request.UserId);
            if (!postExists || !userExists) throw new Exception("Error like");

            var like = await context.Likes.FirstOrDefaultAsync(a => a.PublicationId == request.PublicationId
            && a.LikedById == request.UserId);

            if (like != null)
            {
                context.Likes.Remove(like);
            }
            else
            {
                await context.Likes.AddAsync(new Like
                {
                    LikedById = request.UserId,
                    PublicationId = request.PublicationId
                });
                isLikedByUser = true;
            }
            bool sucess = await context.SaveChangesAsync() > 0;
            if (sucess)
            {
                int countOfLikes = await context.Publications
                    .Include(a => a.Likes).Where(a => a.Id == request.PublicationId)
                    .Select(a => a.Likes.Count()).FirstOrDefaultAsync();

                LikeDto result = new LikeDto
                {
                    AmountOfLikes = countOfLikes,
                    IsLikedByCurrentUser = isLikedByUser
                };
                return result;
            }
            throw new Exception("Error from db");
        }
    }
}
