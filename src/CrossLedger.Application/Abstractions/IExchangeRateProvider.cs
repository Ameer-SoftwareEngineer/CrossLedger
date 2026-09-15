using CrossLedger.Application.Fx;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

/// <summary>
/// A source of mid-market exchange rates. Implementations never leak their own
/// provider's response shape past this boundary — every implementation maps into
/// <see cref="ExchangeRateReading"/> before returning (specification 2.5).
/// </summary>
public interface IExchangeRateProvider
{
    Task<ExchangeRateReading> GetRateAsync(Currency from, Currency to, CancellationToken cancellationToken);
}
