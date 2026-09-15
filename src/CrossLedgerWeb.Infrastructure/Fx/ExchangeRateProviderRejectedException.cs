namespace CrossLedgerWeb.Infrastructure.Fx;

/// <summary>
/// The provider responded successfully at the HTTP level but reported it cannot serve
/// this rate (bad API key, unsupported currency pair). This is deliberately excluded
/// from ResilientExchangeRateProvider's retry predicate: retrying an invalid API key or
/// an unsupported pair fails identically every time, so it should go straight to the
/// fallback provider instead of burning several seconds of exponential backoff first.
/// </summary>
public sealed class ExchangeRateProviderRejectedException : Exception
{
    public ExchangeRateProviderRejectedException(string message) : base(message)
    {
    }
}
