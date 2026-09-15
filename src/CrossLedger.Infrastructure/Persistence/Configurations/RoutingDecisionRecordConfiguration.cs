using CrossLedger.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedger.Infrastructure.Persistence.Configurations;

public sealed class RoutingDecisionRecordConfiguration : IEntityTypeConfiguration<RoutingDecisionRecord>
{
    public void Configure(EntityTypeBuilder<RoutingDecisionRecord> builder)
    {
        builder.ToTable("RoutingDecisions");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.ProviderCode).HasMaxLength(20);
        builder.Property(r => r.FeeCurrency).HasMaxLength(3);
        builder.Property(r => r.FeeAmount).HasColumnType("decimal(19,4)");
        builder.Property(r => r.Score).HasColumnType("decimal(9,8)");

        builder.HasIndex(r => r.TransferId);
    }
}
