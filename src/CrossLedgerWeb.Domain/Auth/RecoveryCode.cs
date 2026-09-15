using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Auth;

/// <summary>One of the ten single-use codes issued at TOTP enrolment (specification
/// 6.1). Only the hash is persisted, matching RefreshToken's TokenHash pattern -
/// consumption is atomic via Consume(), which rejects a second use of the same code.</summary>
public sealed class RecoveryCode
{
    public RecoveryCodeId Id { get; }
    public UserId UserId { get; }
    public string CodeHash { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UsedAt { get; private set; }

    public RecoveryCode(RecoveryCodeId id, UserId userId, string codeHash, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException("A recovery code hash is required.", nameof(codeHash));

        Id = id;
        UserId = userId;
        CodeHash = codeHash;
        CreatedAt = createdAt;
    }

    public bool IsUsed => UsedAt is not null;

    public void Consume(DateTimeOffset usedAt)
    {
        if (IsUsed)
            throw new RecoveryCodeAlreadyUsedException(Id);

        UsedAt = usedAt;
    }
}
