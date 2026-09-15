using FluentValidation.Results;

namespace CrossLedgerWeb.Application.Exceptions;

/// <summary>Thrown by <c>ValidationBehavior</c> when a request fails its FluentValidation
/// rules. Carries structured errors so the API layer can map it straight to a 400
/// problem-details response without re-parsing anything.</summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }
}
