using CrossLedgerWeb.Domain.Payments;

namespace CrossLedgerWeb.Application.Exceptions;

public sealed class PayoutNotFoundException : Exception
{
    public ProviderCode ProviderCode { get; }
    public string ProviderReference { get; }

    public PayoutNotFoundException(ProviderCode providerCode, string providerReference)
        : base($"No payout found for {providerCode} reference '{providerReference}'.")
    {
        ProviderCode = providerCode;
        ProviderReference = providerReference;
    }
}
