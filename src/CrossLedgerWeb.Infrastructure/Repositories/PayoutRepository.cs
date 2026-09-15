using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Payments;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class PayoutRepository : IPayoutRepository
{
    private readonly CrossLedgerWebDbContext _db;

    public PayoutRepository(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task<Payout?> GetByIdAsync(PayoutId id, CancellationToken cancellationToken) =>
        _db.Payouts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payout?> GetByProviderReferenceAsync(ProviderCode providerCode, string providerReference, CancellationToken cancellationToken) =>
        _db.Payouts.FirstOrDefaultAsync(
            p => p.ProviderCode == providerCode && p.ProviderReference == providerReference,
            cancellationToken);

    public void Add(Payout payout) => _db.Payouts.Add(payout);
}
