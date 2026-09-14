using CrossLedger.Domain.Fx;
using CrossLedger.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedger.Infrastructure.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasConversion(new QuoteIdConverter()).ValueGeneratedNever();
        builder.Property(q => q.FromCurrency).HasConversion(new CurrencyConverter()).HasColumnName("FromCurrencyCode").HasMaxLength(3);
        builder.Property(q => q.ToCurrency).HasConversion(new CurrencyConverter()).HasColumnName("ToCurrencyCode").HasMaxLength(3);
        builder.Property(q => q.MidMarketRate).HasColumnType("decimal(18,8)");
        builder.Property(q => q.SpreadRate).HasColumnType("decimal(9,6)");

        // CustomerRate is persisted, not recomputed on read: it's the rate that was
        // actually quoted at IssuedAt, which is what an audit or dispute needs, even
        // though it's mechanically derivable from MidMarketRate and SpreadRate.
        builder.Property(q => q.CustomerRate).HasColumnType("decimal(18,8)");

        builder.Property(q => q.IssuedAt);
        builder.Property(q => q.ExpiresAt);

        builder.HasIndex(q => q.ExpiresAt);
    }
}
