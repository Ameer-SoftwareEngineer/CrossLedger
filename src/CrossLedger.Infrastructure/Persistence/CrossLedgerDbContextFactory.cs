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

        return new CrossLedgerDbContext(optionsBuilder.Options);
    }
}
