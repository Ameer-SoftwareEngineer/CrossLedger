namespace CrossLedgerWeb.Domain.Payments;

/// <summary>Lives in Domain, not Application, because <see cref="Payout"/> - a Domain
/// aggregate - needs to record which provider settled it, and Domain cannot reference
/// Application types (Clean Architecture's inward-only dependency rule). The Application
/// layer's IPaymentProvider and friends reference this same enum rather than defining
/// their own.</summary>
public enum ProviderCode
{
    Airwallex,
    Rapyd,
    Stripe,
    Simulated,
}
