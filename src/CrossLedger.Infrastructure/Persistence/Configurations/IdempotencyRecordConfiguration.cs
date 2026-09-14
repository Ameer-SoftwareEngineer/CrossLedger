using CrossLedger.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedger.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        // The unique constraint specification 2.3 requires: a repeated key must fail to
        // insert a second row rather than let the request execute twice.
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasMaxLength(128);
        builder.Property(r => r.ResponsePayload).IsRequired();
        builder.Property(r => r.CreatedAt);
    }
}
