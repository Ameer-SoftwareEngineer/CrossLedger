namespace CrossLedgerWeb.Application.Exceptions;

/// <summary>The same code was already presented once within its own validity window -
/// specification 6.3's TOTP replay protection.</summary>
public sealed class TotpCodeReplayedException : Exception
{
    public TotpCodeReplayedException() : base("This authenticator code has already been used.")
    {
    }
}
