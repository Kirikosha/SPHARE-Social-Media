namespace Application.Features.Violations.Commands;

using Core;
using Domain.Entities;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class CreateViolation
{
    public class Command : IRequest<Result<bool>>
    {
        public required Violation Violation { get; set; }
    }
    public class Handler(ApplicationDbContext context) 
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            context.Violations.Add(request.Violation);
            await context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true); 
        }
    }
}
