namespace Application.Features.Violations.Commands;

using Domain.Entities;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class CreateViolation
{
    public class Command : IRequest<bool>
    {
        public required Violation Violation { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            context.Violations.Add(request.Violation);
            return Task.FromResult(true);
        }
    }
}
