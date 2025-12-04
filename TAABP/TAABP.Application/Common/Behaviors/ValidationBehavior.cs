using FluentValidation;
using MediatR;
using TAABP.Application.Common.Errors;

namespace TAABP.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (errors.Count != 0)
        {
            var errorMessages = string.Join("; ", errors.Select(e => e.ErrorMessage));

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
                    .MakeGenericMethod(valueType);

                return (TResponse)failureMethod.Invoke(null, [ValidationErrors.InvalidInput(errorMessages)])!;
            }

            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(ValidationErrors.InvalidInput(errorMessages));
            }

            throw new ValidationException(errors);
        }

        return await next();
    }
}
