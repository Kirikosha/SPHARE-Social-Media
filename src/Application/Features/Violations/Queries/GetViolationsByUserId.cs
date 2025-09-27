namespace Application.Features.Violations.Queries;

using Application.Core;
using AutoMapper;
using Domain.DTOs.ViolationDTOs;
using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetViolationsByUserId
{
    public class Query : IRequest<Result<List<ViolationDto>>>
    {
        public required int UserId { get; set; }
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
