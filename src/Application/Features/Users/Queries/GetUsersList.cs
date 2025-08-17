namespace Application.Features.Users.Queries;

using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersList
{
    public class Query : IRequest<List<AdminUserDto>> { }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, List<AdminUserDto>>
    {
        public async Task<List<AdminUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await context.Users.Include(a => a.ProfileImage).ToListAsync(cancellationToken);
            return mapper.Map<List<AdminUserDto>>(users);
        }
    }
}
