using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CrossLedger.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef migrations add` build the model and generate migrations
/// without a running host or a reachable database - only used at design time.</summary>
public sealed class CrossLedgerDbContextFactory : IDesignTimeDbContextFactory<CrossLedgerDbContext>
{
    public CrossLedgerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrossLedgerDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CrossLedger;Trusted_Connection=True;TrustServerCertificate=True;");

        // Only used to shape the model for migration generation - no data is ever
        // actually protected/unprotected at design time, so any valid provider works;
        // this one is isolated from the real app's key ring by application name alone.
        var dataProtectionProvider = DataProtectionProvider.Create("CrossLedger.DesignTime");

        return new CrossLedgerDbContext(optionsBuilder.Options, dataProtectionProvider);
    }
}
