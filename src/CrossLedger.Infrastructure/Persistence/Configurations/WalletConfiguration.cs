using CrossLedger.Domain.Wallets;
using CrossLedger.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedger.Infrastructure.Persistence.Configurations;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasConversion(new WalletIdConverter()).ValueGeneratedNever();
        builder.Property(w => w.OwnerId).HasConversion(new UserIdConverter());
        builder.Property(w => w.Currency).HasConversion(new CurrencyConverter()).HasMaxLength(3).IsRequired();
        builder.Property(w => w.Kind).HasConversion<string>().HasMaxLength(20);

        // Balance is derived from Entries, never its own column (specification 2.1).
        builder.Ignore(w => w.Balance);

        // Entries is a get-only IReadOnlyList wrapping the private _entries field - EF
        // materializes straight into that field, bypassing the (absent) public setter,
        // so the aggregate stays append-only from the outside even to its own ORM.
        builder.HasMany(w => w.Entries)
            .WithOne()
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(w => w.Entries)
            .HasField("_entries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Optimistic concurrency token for the wallet aggregate (specification 2.4).
        // A shadow property - the domain has no reason to know its own storage engine
        // uses SQL Server's rowversion for conflict detection.
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
