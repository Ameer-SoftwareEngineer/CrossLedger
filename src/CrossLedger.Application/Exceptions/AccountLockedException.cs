namespace CrossLedger.Application.Exceptions;

public sealed class AccountLockedException : Exception
{
    public string Email { get; }

    public AccountLockedException(string email)
        : base("This account is temporarily locked after repeated failed sign-in attempts.")
    {
        Email = email;
    }
}
