using CrossLedgerWeb.Domain.Payments;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Abstractions;

public interface IPayoutRepository
{
    Task<Payout?> GetByIdAsync(PayoutId id, CancellationToken cancellationToken);

    /// <summary>Correlates an inbound provider webhook back to the Payout it belongs to.</summary>
    Task<Payout?> GetByProviderReferenceAsync(ProviderCode providerCode, string providerReference, CancellationToken cancellationToken);

    /// <summary>Stages a new payout for insertion - committed by UnitOfWorkBehavior's
    /// SaveChanges at the end of the pipeline, not immediately.</summary>
    void Add(Payout payout);
}
