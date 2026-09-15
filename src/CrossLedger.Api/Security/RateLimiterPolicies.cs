namespace CrossLedger.Api.Security;

public static class RateLimiterPolicies
{
    /// <summary>Specification 6.4: "Rate limiting on verification" - applied to the TOTP
    /// enrolment-confirmation and step-up endpoints, both of which check a user-supplied
    /// code against a secret and so are the brute-force surface.</summary>
    public const string TotpVerification = "totp-verification";
}
