namespace CrossLedgerWeb.Shared.Transfers;

public sealed record TransferResponse(
    Guid TransferId,
    decimal SourceAmount,
    string SourceCurrency,
    decimal TargetAmount,
    string TargetCurrency,
    DateTimeOffset PostedAt);
