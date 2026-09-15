using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public interface IUsedTotpCodeRepository
{
    /// <summary>An active (non-expired) record for this exact (user, code) pair means
    /// it was already presented within its own validity window - the replay check from
    /// specification 6.3.</summary>
    Task<bool> IsActiveAsync(UserId userId, string code, DateTimeOffset asOf, CancellationToken cancellationToken);

    void Add(UsedTotpCode usedCode);
}
