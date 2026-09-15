using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Fx;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Fx;

/// <summary>Implements the quote lifecycle from specification 2.2: read the mid-market
/// rate, apply the platform spread, lock it for a short window. The 0.50% spread is the
/// platform's revenue margin, recorded on the quote itself - not silently absorbed.</summary>
public sealed class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, CreateQuoteResult>
{
    private static readonly decimal SpreadRate = 0.005m;
    private static readonly TimeSpan QuoteValidity = TimeSpan.FromSeconds(30);

    private readonly IExchangeRateProvider _rates;
    private readonly IQuoteRepository _quotes;
    private readonly IClock _clock;

    public CreateQuoteCommandHandler(IExchangeRateProvider rates, IQuoteRepository quotes, IClock clock)
    {
        _rates = rates;
        _quotes = quotes;
        _clock = clock;
    }

    public async Task<CreateQuoteResult> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var from = Currency.From(request.FromCurrency);
        var to = Currency.From(request.ToCurrency);

        var reading = await _rates.GetRateAsync(from, to, cancellationToken);
        var now = _clock.UtcNow;

        var quote = new Quote(QuoteId.New(), from, to, reading.MidMarketRate, SpreadRate, now, QuoteValidity);
        _quotes.Add(quote);

        return new CreateQuoteResult(quote.Id, quote.FromCurrency, quote.ToCurrency, quote.CustomerRate, quote.ExpiresAt);
    }
}
