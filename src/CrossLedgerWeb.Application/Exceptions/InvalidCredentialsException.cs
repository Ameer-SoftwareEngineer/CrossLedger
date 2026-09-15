namespace CrossLedgerWeb.Application.Exceptions;

/// <summary>Deliberately carries no detail about which part was wrong (bad email vs bad
/// password) - that distinction is exactly what account enumeration attacks look for.</summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}
