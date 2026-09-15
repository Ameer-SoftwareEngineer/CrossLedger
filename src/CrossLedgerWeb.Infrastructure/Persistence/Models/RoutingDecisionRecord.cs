namespace CrossLedgerWeb.Infrastructure.Persistence.Models;

/// <summary>Pure persistence record for IRoutingAuditLog - the full ranked comparison
/// for a routing decision, not just the winner (specification 5.3), so any transfer's
/// routing can be explained months later.</summary>
public sealed class RoutingDecisionRecord
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public string ProviderCode { get; set; } = string.Empty;

    /// <summary>0 = the winner selected as primary; 1, 2, ... are the ordered fallbacks.</summary>
    public int Rank { get; set; }

    public decimal Score { get; set; }
    public decimal FeeAmount { get; set; }
    public string FeeCurrency { get; set; } = string.Empty;
    public double EstimatedSettlementMinutes { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}
