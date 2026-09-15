using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Abstractions;

public interface IRecoveryCodeRepository
{
    Task<IReadOnlyList<RecoveryCode>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    void AddRange(IEnumerable<RecoveryCode> codes);
}
