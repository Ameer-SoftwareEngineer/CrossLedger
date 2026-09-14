using FluentValidation;
using MediatR;
using ValidationException = CrossLedger.Application.Exceptions.ValidationException;

namespace CrossLedger.Application.Behaviors;

/// <summary>Runs every registered FluentValidation validator for the request before the
/// handler executes, so validation lives as pipeline behaviour rather than scattered
/// through controllers (specification 3.2).</summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next().ConfigureAwait(false);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = results.SelectMany(r => r.Errors).ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next().ConfigureAwait(false);
    }
}
