using FluentValidation;
using MediatR;
using Valgor.Application.Common.Results;

namespace Valgor.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var message = string.Join(" | ", failures.Select(failure => failure.ErrorMessage));

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(Error.Validation(message));
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethods()
                .Single(method => method is { Name: nameof(Result.Failure), IsGenericMethodDefinition: true })
                .MakeGenericMethod(resultType);

            return (TResponse)failureMethod.Invoke(null, [Error.Validation(message)])!;
        }

        throw new ValidationException(failures);
    }
}
