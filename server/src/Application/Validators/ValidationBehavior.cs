using Application.Core;
using FluentValidation;

namespace Application.Validators;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
where TResponse : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken 
            cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0) return await next(cancellationToken);

        var errors = string.Join(" ", failures.Select(f => f.ErrorMessage));

        var genericArg = typeof(TResponse).GetGenericArguments()[0];
        var resultType = typeof(Result<>).MakeGenericType(genericArg);

        var failureResult = resultType.GetMethod("Failure", [typeof(string), typeof(int)])!
            .Invoke(null, [errors, 400]);

        return (TResponse)failureResult!;
    }
}