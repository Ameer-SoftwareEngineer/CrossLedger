using CrossLedger.Domain.Ledger;
using CrossLedger.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedger.Infrastructure.Persistence.Configurations;

public sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(new LedgerEntryIdConverter()).ValueGeneratedNever();
        builder.Property(e => e.TransferId).HasConversion(new TransferIdConverter());
        builder.Property(e => e.WalletId).HasConversion(new WalletIdConverter());
        builder.Property(e => e.Direction).HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.PostedAt);

        // Amount is always a positive magnitude; Direction alone carries the sign
        // (specification: "money is never stored as float"). Money is a struct, so it
        // maps as an EF Core 8 complex type (ComplexProperty), not OwnsOne - OwnsOne
        // requires a reference type.
        builder.ComplexProperty(e => e.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(19,4)");
            money.Property(m => m.Currency).HasConversion(new CurrencyConverter()).HasColumnName("CurrencyCode").HasMaxLength(3);
        });

        // Matches the covering index described in specification 4.4: (WalletId,
        // PostedAt) so balance and statement queries are index-only scans.
        builder.HasIndex(e => new { e.WalletId, e.PostedAt });

        builder.HasIndex(e => e.TransferId);
    }
}
