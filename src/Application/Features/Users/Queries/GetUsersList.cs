namespace Application.Features.Users.Queries;

using AutoMapper;
using Domain.DTOs.UserDTOs;
using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersList
{
    public class Query : IRequest<List<UserDto>> { }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, List<UserDto>>
    {
        public async Task<List<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await context.Users.Include(a => a.ProfileImage).ToListAsync(cancellationToken);
            return mapper.Map<List<UserDto>>(users);
        }
    }
}
