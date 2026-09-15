using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Exceptions;

public sealed class RecoveryCodeAlreadyUsedException : DomainException
{
    public RecoveryCodeId RecoveryCodeId { get; }

    public RecoveryCodeAlreadyUsedException(RecoveryCodeId recoveryCodeId)
        : base($"Recovery code {recoveryCodeId} has already been used.")
    {
        RecoveryCodeId = recoveryCodeId;
    }
}
