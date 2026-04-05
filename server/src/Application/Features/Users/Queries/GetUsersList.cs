using Application.Core.Pagination;

namespace Application.Features.Users.Queries;

using Core;
using Domain.DTOs.UserDTOs;
using Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public class GetUsersList
{
    public class Query : IRequest<Result<PagedList<AdminUserDto>>>
    {
        public required PaginationParams PaginationParams { get; set; }
    }

    public class Handler(ApplicationDbContext context) 
        : IRequestHandler<Query, Result<PagedList<AdminUserDto>>>
    {
        public async Task<Result<PagedList<AdminUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Users
                .OrderBy(u => u.Username)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UniqueNameIdentifier = u.UniqueNameIdentifier,
                    Email = u.Email,
                    ProfileImageUrl = u.ProfileImage != null ? u.ProfileImage.ImageUrl : null,
                    ViolationScore = u.ViolationScore,
                    AmountOfViolations = u.Violations.Count,
                    Blocked = u.Blocked,
                    BlockedAt = u.BlockedAt
                });

            var pagedList = await PagedList<AdminUserDto>.CreateAsync(
                query,
                request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize,
                cancellationToken);

            return Result<PagedList<AdminUserDto>>.Success(pagedList);
        }
    }
}
