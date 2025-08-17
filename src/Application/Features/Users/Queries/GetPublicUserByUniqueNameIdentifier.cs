namespace Application.Features.Users.Queries;

using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicUserByUniqueNameIdentifier
{
    public class Query : IRequest<PublicUserDto>
    {
        public required string UniqueNameIdentifier { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, PublicUserDto>
    {
        public async Task<PublicUserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .Include(a => a.ProfileImage)
                .FirstOrDefaultAsync(a => a.UniqueNameIdentifier == request.UniqueNameIdentifier);

            if (user == null) throw new Exception("User was not found");
            return mapper.Map<PublicUserDto>(user);

        }
    }
}
