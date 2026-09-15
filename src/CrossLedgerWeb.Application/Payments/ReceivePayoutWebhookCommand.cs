using CrossLedgerWeb.Domain.Payments;
using MediatR;

namespace CrossLedgerWeb.Application.Payments;

/// <summary>Signature verification happens before this command is even sent (in the API
/// controller, which is the layer that actually has each provider's webhook secret) -
/// by the time this reaches Application, the payload is already trusted. This command's
/// own job is exactly the three things specification 5.4 calls out: deduplicate on event
/// id, reject an out-of-order regression, and only then touch the ledger.</summary>
public sealed record ReceivePayoutWebhookCommand(
    ProviderCode ProviderCode,
    string EventId,
    string ProviderReference,
    PayoutWebhookEventType EventType) : IRequest<ReceivePayoutWebhookResult>;

public enum PayoutWebhookEventType
{
    Settled,
    Failed,
}

/// <summary>WasDuplicate is true when the event id had already been processed - the
/// caller (a webhook endpoint) should still answer 200 either way, since re-delivery is
/// expected, not an error.</summary>
public sealed record ReceivePayoutWebhookResult(bool WasDuplicate, PayoutState? PayoutState);
