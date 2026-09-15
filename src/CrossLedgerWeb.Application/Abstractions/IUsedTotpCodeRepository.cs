using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Abstractions;

public interface IUsedTotpCodeRepository
{
    /// <summary>An active (non-expired) record for this exact (user, code) pair means
    /// it was already presented within its own validity window - the replay check from
    /// specification 6.3.</summary>
    Task<bool> IsActiveAsync(UserId userId, string code, DateTimeOffset asOf, CancellationToken cancellationToken);

    void Add(UsedTotpCode usedCode);
}
