using Application.Core;
using FluentValidation;

namespace Application.Validators;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0) return await next(cancellationToken);

        var errorMessage = string.Join(" ", failures.Select(f => f.ErrorMessage));

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = typeof(TResponse).GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(genericArg);
            var failureResult = resultType.GetMethod("Failure", [typeof(string), typeof(int)])!
                .Invoke(null, [errorMessage, 400]);
            return (TResponse)failureResult!;
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition()
                .FullName!.StartsWith("OneOf.OneOf`"))
        {
            var error = new Error(errorMessage, 400);

            var implicitOp = typeof(TResponse)
                .GetMethods()
                .FirstOrDefault(m =>
                    m.Name == "op_Implicit" &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(Error));

            if (implicitOp != null)
                return (TResponse)implicitOp.Invoke(null, [error])!;
        }

        throw new ValidationException(failures);
    }
}