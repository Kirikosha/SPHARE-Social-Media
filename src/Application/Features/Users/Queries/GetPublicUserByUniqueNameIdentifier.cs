namespace Application.Features.Users.Queries;

using Core;
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
                .Include(a => a.Address)
                .Include(a => a.ProfileDetails)
                .FirstOrDefaultAsync(a => a.UniqueNameIdentifier == request.UniqueNameIdentifier,
                    cancellationToken);

            return user == null ? Result<PublicUserDto>.Failure("User was not found", 404) 
                : Result<PublicUserDto>.Success(mapper.Map<PublicUserDto>(user));
        }
    }
}
