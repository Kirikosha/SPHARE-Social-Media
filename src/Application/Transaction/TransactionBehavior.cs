namespace Application.Transaction;

using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ApplicationDbContext context;
    public TransactionBehavior(ApplicationDbContext context)
    {
        this.context = context;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction != null)
        {
            return await next();
        }

        TResponse response;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            response = await next();
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return response;
    }
}
