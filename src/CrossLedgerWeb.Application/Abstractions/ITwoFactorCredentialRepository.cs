using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Abstractions;

public interface ITwoFactorCredentialRepository
{
    Task<TwoFactorCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    void Add(TwoFactorCredential credential);
}
