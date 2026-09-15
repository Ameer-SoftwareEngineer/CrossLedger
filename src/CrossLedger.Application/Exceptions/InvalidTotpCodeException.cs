namespace CrossLedger.Application.Exceptions;

public sealed class InvalidTotpCodeException : Exception
{
    public InvalidTotpCodeException() : base("The authenticator code is invalid.")
    {
    }
}
