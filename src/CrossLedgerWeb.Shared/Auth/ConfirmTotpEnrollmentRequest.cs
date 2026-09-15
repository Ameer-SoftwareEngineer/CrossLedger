namespace CrossLedgerWeb.Shared.Auth;

/// <summary>The client holds the secret from BeginTotpEnrollmentResponse and sends it
/// back here alongside the first generated code - specification 6.1's enrolment
/// integrity means the server never persists a secret it hasn't seen proven.</summary>
public sealed record ConfirmTotpEnrollmentRequest(string Secret, string Code);

public sealed record ConfirmTotpEnrollmentResponse(IReadOnlyList<string> RecoveryCodes);
