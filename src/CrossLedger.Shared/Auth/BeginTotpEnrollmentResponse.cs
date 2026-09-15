namespace CrossLedger.Shared.Auth;

public sealed record BeginTotpEnrollmentResponse(string Secret, string QrCodeUri);
