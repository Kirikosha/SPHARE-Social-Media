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
    public class Query : IRequest<Result<List<PublicUserDto>>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<List<PublicUserDto>>>
    {
        public async Task<Result<List<PublicUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UniqueNameIdentifier)) 
                return Result<List<PublicUserDto>>.Success(new List<PublicUserDto>());

            var users = await context.Users
                .Include(a => a.ProfileImage)
                .Where(a => a.UniqueNameIdentifier.Contains(request.UniqueNameIdentifier))
                .ProjectTo<PublicUserDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<PublicUserDto>>.Success(users);
        }
    }
}
