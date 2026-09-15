namespace CrossLedger.Application.Exceptions;

public sealed class TwoFactorNotEnabledException : Exception
{
    public TwoFactorNotEnabledException() : base("Two-factor authentication is not enabled for this account.")
    {
    }
}
