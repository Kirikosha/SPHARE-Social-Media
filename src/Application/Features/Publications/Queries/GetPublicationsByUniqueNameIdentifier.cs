using Application.Core.Pagination;

namespace Application.Features.Publications.Queries;

using Core;
using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.PublicationDTOs;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsByUniqueNameIdentifier
{
    public class Query : IRequest<Result<PagedList<PublicationCardDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
        public required string UserId { get; set; }
        public PaginationParams PaginationParams { get; set; } = new();
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<PagedList<PublicationCardDto>>>
    {
        public async Task<Result<PagedList<PublicationCardDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userExists = await context.Users
                .AnyAsync(u => u.Id == request.UserId, cancellationToken);

            if (!userExists)
                return Result<PagedList<PublicationCardDto>>
                    .Failure("User was not found", 404);
            
            var query = context.Publications
                .AsNoTracking()
                .Where(p => p.Author.UniqueNameIdentifier == request.UniqueNameIdentifier)
                .OrderByDescending(p => p.PostedAt)
                .Select(p => new PublicationCardDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostedAt = p.PostedAt,
                    UpdatedAt = p.UpdatedAt,

                    ImageUrls = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList(),

                    Author = new PublicUserBriefDto
                    {
                        Id = p.Author.Id,
                        Username = p.Author.Username,
                        UniqueNameIdentifier = p.Author.UniqueNameIdentifier,
                        Blocked = p.Author.Blocked,
                        ImageUrl = p.Author.ProfileImage != null
                            ? p.Author.ProfileImage.ImageUrl
                            : null
                    },

                    LikesAmount = p.Likes.Count(),

                    IsLikedByCurrentUser = p.Likes
                        .Any(l => l.LikedById == request.UserId),

                    CommentAmount = p.Comments.Count(),

                    PublicationType = p.PublicationType,
                    ViewCount = p.ViewCount,
                    IsDeleted = p.IsDeleted
                });
            
            var pagedResult = await PagedList<PublicationCardDto>
                .CreateAsync(
                    query,
                    request.PaginationParams.PageNumber,
                    request.PaginationParams.PageSize,
                    cancellationToken
                );

            return Result<PagedList<PublicationCardDto>>
                .Success(pagedResult);
        }
    }
}
