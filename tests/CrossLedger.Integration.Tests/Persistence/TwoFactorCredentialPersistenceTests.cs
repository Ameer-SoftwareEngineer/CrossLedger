using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Integration.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Integration.Tests.Persistence;

// Proves the TOTP secret is actually encrypted at rest (specification 6.4), not just
// round-tripped - a SQLite in-memory DB lets us read the raw stored column directly and
// assert it is neither the plaintext secret nor readable without the same protector.
public sealed class TwoFactorCredentialPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CrossLedgerDbContext> _options;

    public TwoFactorCredentialPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<CrossLedgerDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new CrossLedgerDbContext(_options, TestDataProtection.Provider);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task A_credentials_secret_round_trips_decrypted_through_the_same_protector()
    {
        var userId = UserId.New();
        const string plainSecret = "JBSWY3DPEHPK3PXP";
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), userId, plainSecret, DateTimeOffset.UtcNow);

        await using (var writeContext = new CrossLedgerDbContext(_options, TestDataProtection.Provider))
        {
            writeContext.TwoFactorCredentials.Add(credential);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new CrossLedgerDbContext(_options, TestDataProtection.Provider);
        var reloaded = await readContext.TwoFactorCredentials.SingleAsync(c => c.UserId == userId);

        reloaded.Secret.Should().Be(plainSecret);
    }

    [Fact]
    public async Task The_secret_is_not_stored_in_plaintext_in_the_underlying_column()
    {
        var userId = UserId.New();
        const string plainSecret = "JBSWY3DPEHPK3PXP";
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), userId, plainSecret, DateTimeOffset.UtcNow);

        await using (var writeContext = new CrossLedgerDbContext(_options, TestDataProtection.Provider))
        {
            writeContext.TwoFactorCredentials.Add(credential);
            await writeContext.SaveChangesAsync();
        }

        await using var checkContext = new CrossLedgerDbContext(_options, TestDataProtection.Provider);
        var rawSecret = await checkContext.Database
            .SqlQueryRaw<string>("SELECT Secret AS Value FROM TwoFactorCredentials LIMIT 1")
            .SingleAsync();

        rawSecret.Should().NotBe(plainSecret);
    }
}
