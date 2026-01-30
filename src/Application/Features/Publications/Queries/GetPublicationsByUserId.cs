namespace Application.Features.Publications.Queries;

using Application.Core;
using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.PublicationDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUserId
{
    public class Query : IRequest<Result<List<PublicationDto>>>
    {
        public required int UserId { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<List<PublicationDto>>>
    {
        public async Task<Result<List<PublicationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publications = await context.Publications
                .Include(a => a.Likes)
                .Include(a => a.Images)
                .Include(a => a.Comments)
                .Include(a => a.Author).ThenInclude(a => a.ProfileImage)
                .Where(a => a.AuthorId == request.UserId)
                .Select(p => new PublicationDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,
                    RemindAt = p.RemindAt,
                    Images = mapper.Map<List<ImageDto>>(p.Images),
                    Author = mapper.Map<PublicUserDto>(p.Author),
                    LikesAmount = p.Likes.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(a => a.LikedById == request.UserId),
                    CommentAmount = context.Comments.Count(c => c.PublicationId == p.Id)
                }).ToListAsync(cancellationToken);
            return Result<List<PublicationDto>>.Success(publications);
        }
    }
}
