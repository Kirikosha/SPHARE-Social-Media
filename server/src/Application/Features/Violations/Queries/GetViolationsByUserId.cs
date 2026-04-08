using Application.DTOs.ViolationDTOs;
using Application.Interfaces.Services;

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
    public class Handler(IViolationService violationService) : IRequestHandler<Query, Result<List<ViolationDto>>>
    {
        public async Task<Result<List<ViolationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var violations = await violationService.GetViolationsByUserId(request.UserId, cancellationToken);
            return Result<List<ViolationDto>>.Success(violations);
        }
    }
}
