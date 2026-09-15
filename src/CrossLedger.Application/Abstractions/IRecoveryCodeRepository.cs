using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public interface IRecoveryCodeRepository
{
    Task<IReadOnlyList<RecoveryCode>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    void AddRange(IEnumerable<RecoveryCode> codes);
}
