using Application.Interfaces.Services;

namespace Application.Features.Violations.Commands;

using Core;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class CreateViolation
{
    public class Command : IRequest<Result<bool>>
    {
        public required Violation Violation { get; set; }
    }
    public class Handler(IViolationService violationService) 
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var res = await violationService.CreateViolation(request.Violation, cancellationToken);
            return res ? Result<bool>.Success(true) : Result<bool>.Failure("Something went wrong", 400);
        }
    }
}
