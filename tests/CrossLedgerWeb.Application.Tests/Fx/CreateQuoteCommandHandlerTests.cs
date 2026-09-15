using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Domain.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Fx;

public class CreateQuoteCommandHandlerTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IExchangeRateProvider> _rates = new();
    private readonly Mock<IQuoteRepository> _quotes = new();
    private readonly Mock<IClock> _clock = new();

    private CreateQuoteCommandHandler CreateHandler() => new(_rates.Object, _quotes.Object, _clock.Object);

    [Fact]
    public async Task Handle_locks_the_mid_market_rate_with_the_platform_spread_applied()
    {
        // specification section 2.2: mid-market 279.90, spread 0.50% -> customer rate ~278.50
        _rates.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRateReading(Usd, Pkr, 279.90m, Now, IsStale: false));
        _clock.Setup(x => x.UtcNow).Returns(Now);
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateQuoteCommand("USD", "PKR", 100m), CancellationToken.None);

        result.Rate.Should().Be(278.5005m);
        result.ExpiresAt.Should().Be(Now.AddSeconds(30));
        result.FromCurrency.Should().Be(Usd);
        result.ToCurrency.Should().Be(Pkr);
    }

    [Fact]
    public async Task Handle_stages_the_quote_for_insertion()
    {
        _rates.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRateReading(Usd, Pkr, 279.90m, Now, IsStale: false));
        _clock.Setup(x => x.UtcNow).Returns(Now);
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateQuoteCommand("USD", "PKR", 100m), CancellationToken.None);

        _quotes.Verify(x => x.Add(It.Is<Quote>(q => q.Id == result.QuoteId)), Times.Once);
    }
}
