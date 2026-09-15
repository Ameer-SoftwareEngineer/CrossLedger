namespace CrossLedgerWeb.Api.Webhooks;

/// <summary>
/// The webhook body shape this platform expects from a payout provider. Deliberately a
/// single normalized envelope rather than four separate real Airwallex/Rapyd/Stripe
/// payload parsers: this repository has no live sandbox access to verify what those
/// providers' actual inbound webhook JSON looks like (their QuoteAsync/ExecuteAsync
/// request shapes carry the same caveat - see each provider's own doc comments), so
/// writing bespoke parsers for undocumented-here payloads would be unverifiable,
/// speculative code. A real per-provider adapter that translates each provider's actual
/// webhook format into this envelope is the acknowledged next step; this is what lets
/// signature verification, deduplication and the state-machine wiring be built and
/// genuinely tested today, against SimulatedProvider, rather than blocked on it.
/// </summary>
public sealed record PayoutWebhookEnvelope(string EventId, string PayoutReference, string Status);
