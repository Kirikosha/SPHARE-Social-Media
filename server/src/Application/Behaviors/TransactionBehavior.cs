using Application.Core;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_unitOfWork.HasActiveTransaction)
            return await next(ct);

        using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next(ct);

            if (response is IResult result && !result.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return response;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {RequestType}", typeof(TRequest).Name);
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
