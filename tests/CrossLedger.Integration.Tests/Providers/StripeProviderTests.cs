using CrossLedger.Providers.Common;
using CrossLedger.Providers.Stripe;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CrossLedger.Integration.Tests.Providers;

// Unlike Quote/Execute (see StripeProvider's doc comment), this signature scheme is
// Stripe's real, stable, publicly documented algorithm - correct without a live account.
public class StripeProviderTests
{
    private static StripeProvider CreateProvider() =>
        new(new HttpClient { BaseAddress = new Uri("https://api.stripe.com/") }, Options.Create(new StripeOptions()));

    private static string BuildStripeSignatureHeader(string payload, string secret, long unixTimestamp)
    {
        var signedPayload = $"{unixTimestamp}.{payload}";
        var v1 = HmacSignatureVerifier.Sign(signedPayload, secret);
        return $"t={unixTimestamp},v1={v1}";
    }

    [Fact]
    public void Accepts_a_correctly_formed_stripe_signature_header()
    {
        var provider = CreateProvider();
        const string payload = "{\"type\":\"charge.succeeded\"}";
        var header = BuildStripeSignatureHeader(payload, "whsec_test", 1893456000);

        provider.VerifySignature(payload, header, "whsec_test").Should().BeTrue();
    }

    [Fact]
    public void Rejects_a_signature_computed_without_the_timestamp_prefix()
    {
        var provider = CreateProvider();
        const string payload = "{\"type\":\"charge.succeeded\"}";
        // A naive HMAC over just the payload (like the other providers use) rather than
        // "{timestamp}.{payload}" - must NOT verify against Stripe's actual scheme.
        var bareSignature = HmacSignatureVerifier.Sign(payload, "whsec_test");
        var header = $"t=1893456000,v1={bareSignature}";

        provider.VerifySignature(payload, header, "whsec_test").Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-the-right-shape")]
    [InlineData("t=123")]
    public void Rejects_a_malformed_header_instead_of_throwing(string malformed)
    {
        var provider = CreateProvider();

        var act = () => provider.VerifySignature("payload", malformed, "secret");

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    [Fact]
    public void Supports_never_returns_true_stripe_is_the_funding_rail_not_a_payout_route()
    {
        var provider = CreateProvider();
        var corridor = new CrossLedger.Application.Payments.Corridor(
            CrossLedger.Domain.ValueObjects.Currency.From("USD"),
            CrossLedger.Domain.ValueObjects.Currency.From("PKR"),
            "PK");

        provider.Supports(corridor).Should().BeFalse();
    }
}
