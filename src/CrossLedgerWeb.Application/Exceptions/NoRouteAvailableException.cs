using CrossLedgerWeb.Application.Payments;

namespace CrossLedgerWeb.Application.Exceptions;

/// <summary>No registered provider both supports the corridor and is currently
/// healthy (specification 5.3's two hard filters).</summary>
public sealed class NoRouteAvailableException : Exception
{
    public Corridor Corridor { get; }

    public NoRouteAvailableException(Corridor corridor)
        : base($"No payment provider is available for {corridor.SourceCurrency.Code} -> {corridor.TargetCurrency.Code} ({corridor.DestinationCountry}).")
    {
        Corridor = corridor;
    }
}
