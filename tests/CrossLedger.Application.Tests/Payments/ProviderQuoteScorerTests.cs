using CrossLedger.Application.Payments;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Application.Tests.Payments;

public class ProviderQuoteScorerTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private readonly ProviderQuoteScorer _scorer = new();

    private static ProviderQuote Quote(decimal fee, TimeSpan settlement) =>
        new(ProviderCode.Airwallex, new Money(1000m, Usd), new Money(fee, Usd), settlement);

    [Fact]
    public void A_cheaper_quote_scores_higher_than_a_more_expensive_one_at_equal_speed_and_stats()
    {
        var cheap = Quote(fee: 1m, settlement: TimeSpan.FromHours(1));
        var expensive = Quote(fee: 20m, settlement: TimeSpan.FromHours(1));
        var stats = ProviderStats.Unknown;

        _scorer.Score(cheap, stats, RoutingPreference.Standard)
            .Should().BeGreaterThan(_scorer.Score(expensive, stats, RoutingPreference.Standard));
    }

    [Fact]
    public void A_faster_quote_scores_higher_than_a_slower_one_at_equal_cost_and_stats()
    {
        var fast = Quote(fee: 5m, settlement: TimeSpan.FromMinutes(10));
        var slow = Quote(fee: 5m, settlement: TimeSpan.FromHours(20));
        var stats = ProviderStats.Unknown;

        _scorer.Score(fast, stats, RoutingPreference.Standard)
            .Should().BeGreaterThan(_scorer.Score(slow, stats, RoutingPreference.Standard));
    }

    [Fact]
    public void Priority_preference_weighs_speed_more_heavily_than_standard()
    {
        var fast = Quote(fee: 5m, settlement: TimeSpan.FromMinutes(1));
        var slow = Quote(fee: 5m, settlement: TimeSpan.FromHours(12));
        var stats = ProviderStats.Unknown;

        var standardGap = _scorer.Score(fast, stats, RoutingPreference.Standard)
            - _scorer.Score(slow, stats, RoutingPreference.Standard);
        var priorityGap = _scorer.Score(fast, stats, RoutingPreference.Priority)
            - _scorer.Score(slow, stats, RoutingPreference.Priority);

        priorityGap.Should().BeGreaterThan(standardGap);
    }

    [Fact]
    public void Higher_reliability_and_headroom_score_higher_at_identical_quotes()
    {
        var quote = Quote(fee: 5m, settlement: TimeSpan.FromHours(2));
        var strong = new ProviderStats(RollingSuccessRate: 0.99m, RemainingDailyHeadroomRatio: 0.8m);
        var weak = new ProviderStats(RollingSuccessRate: 0.40m, RemainingDailyHeadroomRatio: 0.1m);

        _scorer.Score(quote, strong, RoutingPreference.Standard)
            .Should().BeGreaterThan(_scorer.Score(quote, weak, RoutingPreference.Standard));
    }

    [Fact]
    public void Score_never_exceeds_one_even_for_a_free_instant_perfect_provider()
    {
        var perfect = Quote(fee: 0m, settlement: TimeSpan.Zero);
        var stats = new ProviderStats(RollingSuccessRate: 1m, RemainingDailyHeadroomRatio: 1m);

        _scorer.Score(perfect, stats, RoutingPreference.Standard).Should().Be(1m);
    }

    [Fact]
    public void A_settlement_time_at_or_beyond_the_slowest_reasonable_ceiling_scores_zero_for_speed()
    {
        var atCeiling = Quote(fee: 0m, settlement: TimeSpan.FromHours(24));
        var beyondCeiling = Quote(fee: 0m, settlement: TimeSpan.FromHours(48));
        var stats = new ProviderStats(RollingSuccessRate: 0m, RemainingDailyHeadroomRatio: 0m);

        _scorer.Score(atCeiling, stats, RoutingPreference.Standard)
            .Should().Be(_scorer.Score(beyondCeiling, stats, RoutingPreference.Standard));
    }
}
