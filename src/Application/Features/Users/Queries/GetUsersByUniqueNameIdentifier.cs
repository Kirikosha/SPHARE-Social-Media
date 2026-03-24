namespace Application.Features.Users.Queries;

using Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersByUniqueNameIdentifier
{
    public class Query : IRequest<Result<List<PublicUserBriefDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<List<PublicUserBriefDto>>>
    {
        public async Task<Result<List<PublicUserBriefDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UniqueNameIdentifier))
                return Result<List<PublicUserBriefDto>>.Success(new List<PublicUserBriefDto>());

            var searchTerm = request.UniqueNameIdentifier;

            var users = await context.Users
                .Where(u => EF.Functions.TrigramsSimilarity(u.UniqueNameIdentifier, searchTerm) > 0.3)
                .OrderByDescending(u => EF.Functions.TrigramsSimilarity(u.UniqueNameIdentifier, searchTerm))
                .Select(u => new PublicUserBriefDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UniqueNameIdentifier = u.UniqueNameIdentifier,
                    ImageUrl = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
                    Blocked = u.Blocked
                })
                .ToListAsync(cancellationToken);

            return Result<List<PublicUserBriefDto>>.Success(users);
        }
    }
}
