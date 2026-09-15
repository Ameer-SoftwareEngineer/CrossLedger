namespace CrossLedgerWeb.Shared.Auth;

/// <summary>Operation is a string on the wire (e.g. "HighValueTransfer") rather than the
/// server's StepUpOperation enum, so Shared - referenced by the Blazor client too - never
/// needs to depend on the server-only Application layer.</summary>
public sealed record RequestStepUpTokenRequest(string Operation, string Code);

public sealed record StepUpTokenResponse(string StepUpToken, DateTimeOffset ExpiresAt);
