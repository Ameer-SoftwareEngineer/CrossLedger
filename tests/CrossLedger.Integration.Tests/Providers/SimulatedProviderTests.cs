using CrossLedger.Application.Payments;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Providers.Common;
using CrossLedger.Providers.Simulated;
using FluentAssertions;

namespace CrossLedger.Integration.Tests.Providers;

public class SimulatedProviderTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Corridor UsToPk = new(Usd, Currency.From("PKR"), "PK");

    private static PayoutRequest Request(string idempotencyKey = "key-1") =>
        new(TransferId.New(), UsToPk, new Money(1000m, Usd), idempotencyKey, RoutingPreference.Standard);

    [Fact]
    public void Supports_every_corridor_by_design()
    {
        var provider = new SimulatedProvider();

        provider.Supports(UsToPk).Should().BeTrue();
    }

    [Fact]
    public async Task QuoteAsync_applies_the_configured_fee_ratio()
    {
        var provider = new SimulatedProvider(new SimulatedProviderOptions { FeeRatio = 0.02m });

        var quote = await provider.QuoteAsync(Request(), CancellationToken.None);

        quote.Fee.Amount.Should().Be(20.00m);
    }

    [Fact]
    public async Task ExecuteAsync_accepts_by_default()
    {
        var provider = new SimulatedProvider();

        var result = await provider.ExecuteAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(PayoutOutcome.Accepted);
        result.ProviderReference.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_rejects_every_payout_when_configured_to()
    {
        var provider = new SimulatedProvider(new SimulatedProviderOptions { RejectAllPayouts = true });

        var result = await provider.ExecuteAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(PayoutOutcome.Rejected);
    }

    [Fact]
    public async Task ExecuteAsync_is_idempotent_on_the_idempotency_key()
    {
        var provider = new SimulatedProvider();
        var request = Request("same-key");

        var first = await provider.ExecuteAsync(request, CancellationToken.None);
        var second = await provider.ExecuteAsync(request, CancellationToken.None);

        // A fresh execution would mint a new ProviderReference each time - an identical
        // one proves the second call replayed the recorded outcome instead of re-running.
        second.ProviderReference.Should().Be(first.ProviderReference);
    }

    [Fact]
    public async Task CheckHealthAsync_reflects_the_configured_status()
    {
        var provider = new SimulatedProvider(new SimulatedProviderOptions { HealthStatus = ProviderHealthStatus.Unavailable });

        var health = await provider.CheckHealthAsync(CancellationToken.None);

        health.Status.Should().Be(ProviderHealthStatus.Unavailable);
    }

    [Fact]
    public void VerifySignature_accepts_a_correctly_signed_payload()
    {
        var provider = new SimulatedProvider(new SimulatedProviderOptions { WebhookSecret = "sim-secret" });
        var signature = HmacSignatureVerifier.Sign("payload", "sim-secret");

        provider.VerifySignature("payload", signature, "sim-secret").Should().BeTrue();
    }
}
