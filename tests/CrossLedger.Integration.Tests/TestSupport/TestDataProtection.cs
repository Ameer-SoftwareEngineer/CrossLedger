using Microsoft.AspNetCore.DataProtection;

namespace CrossLedger.Integration.Tests.TestSupport;

// CrossLedgerDbContext needs an IDataProtectionProvider to encrypt TOTP secrets at
// rest. This resolves to the machine's default key ring, isolated from the real app's
// protector (and from other test providers below) purely by application-name
// discriminator - good enough for a correctness round trip, not meant to model
// production key management (that's Key Vault, per specification 3.4).
internal static class TestDataProtection
{
    public static IDataProtectionProvider Provider { get; } = DataProtectionProvider.Create("CrossLedger.Tests");
}
