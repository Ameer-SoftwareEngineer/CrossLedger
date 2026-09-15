using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Exceptions;

/// <summary>Every live provider failed and no cached reading exists to fall back on
/// (specification 2.5's stale-if-error path only helps once something has been cached
/// at least once).</summary>
public sealed class ExchangeRateUnavailableException : Exception
{
    public Currency From { get; }
    public Currency To { get; }

    public ExchangeRateUnavailableException(Currency from, Currency to, Exception innerException)
        : base($"No exchange rate is available for {from.Code}/{to.Code}.", innerException)
    {
        From = from;
        To = to;
    }
}
