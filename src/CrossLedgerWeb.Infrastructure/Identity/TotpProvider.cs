using CrossLedgerWeb.Application.Abstractions;
using OtpNet;

namespace CrossLedgerWeb.Infrastructure.Identity;

/// <summary>RFC 6238 time-based codes via Otp.NET (specification 6.1), compatible with
/// Google Authenticator and Authy.</summary>
public sealed class TotpProvider : ITotpProvider
{
    private const int SecretLengthBytes = 20; // 160 bits - RFC 4226/6238's recommended key length

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretLengthBytes);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string secret, string accountEmail, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccount = Uri.EscapeDataString(accountEmail);

        return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));

        // +/-1 step tolerates ordinary clock drift between server and authenticator
        // device without materially widening the window an attacker could brute-force.
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
