namespace CrossLedgerWeb.Application.Abstractions;

/// <summary>Wraps Otp.NET's RFC 6238 implementation (specification 6.1) behind an
/// interface so Application never references the library directly.</summary>
public interface ITotpProvider
{
    string GenerateSecret();

    /// <summary>An otpauth:// URI an authenticator app can turn into a QR code.</summary>
    string GenerateQrCodeUri(string secret, string accountEmail, string issuer);

    bool VerifyCode(string secret, string code);
}
