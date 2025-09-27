namespace Application.Features.Users.Queries;

using Application.Core;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicUserByUniqueNameIdentifier
{
    public class Query : IRequest<Result<PublicUserDto>>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<PublicUserDto>>
    {
        public async Task<Result<PublicUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .Include(a => a.ProfileImage)
                .FirstOrDefaultAsync(a => a.UniqueNameIdentifier == request.UniqueNameIdentifier);

            if (user == null)
                return Result<PublicUserDto>.Failure("User was not found", 404);
            return Result<PublicUserDto>.Success(mapper.Map<PublicUserDto>(user));

        }
    }
}
