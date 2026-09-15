namespace CrossLedger.Application.Exceptions;

public sealed class RegistrationFailedException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public RegistrationFailedException(IReadOnlyList<string> errors)
        : base("Registration failed: " + string.Join("; ", errors))
    {
        Errors = errors;
    }
}
