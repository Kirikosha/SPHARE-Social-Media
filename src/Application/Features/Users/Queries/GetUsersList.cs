namespace Application.Features.Users.Queries;

using Core;
using AutoMapper;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersList
{
    public class Query : IRequest<Result<List<AdminUserDto>>> { }

    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<List<AdminUserDto>>>
    {
        public async Task<Result<List<AdminUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await context.Users.Include(a => a.ProfileImage).ToListAsync(cancellationToken);
            return Result<List<AdminUserDto>>.Success(mapper.Map<List<AdminUserDto>>(users));
        }
    }
}
