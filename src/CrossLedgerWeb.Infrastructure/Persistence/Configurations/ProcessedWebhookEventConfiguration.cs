using CrossLedgerWeb.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedgerWeb.Infrastructure.Persistence.Configurations;

public sealed class ProcessedWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("ProcessedWebhookEvents");

        // Composite key: event ids are only unique within one provider's own id space.
        builder.HasKey(e => new { e.ProviderCode, e.EventId });
        builder.Property(e => e.ProviderCode).HasMaxLength(20);
        builder.Property(e => e.EventId).HasMaxLength(256);
        builder.Property(e => e.ProcessedAt);
    }
}
