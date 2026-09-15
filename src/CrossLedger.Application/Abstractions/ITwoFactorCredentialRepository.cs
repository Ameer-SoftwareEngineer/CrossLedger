using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public interface ITwoFactorCredentialRepository
{
    Task<TwoFactorCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    void Add(TwoFactorCredential credential);
}
