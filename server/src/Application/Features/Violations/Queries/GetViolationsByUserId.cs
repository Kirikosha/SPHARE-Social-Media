using Application.DTOs.ViolationDTOs;

namespace Application.Features.Violations.Queries;

using Core;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetViolationsByUserId
{
    public class Query : IRequest<Result<List<ViolationDto>>>
    {
        public required string UserId { get; set; }
    }
    public class Handler(ApplicationDbContext context, IMapper mapper) 
        : IRequestHandler<Query, Result<List<ViolationDto>>>
    {
        public async Task<Result<List<ViolationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            List<Violation> violations = await context.Violations
                .Where(a => a.ViolatedById == request.UserId).ToListAsync(cancellationToken);
            return Result<List<ViolationDto>>.Success(mapper.Map<List<ViolationDto>>(violations));
        }
    }
}
