using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class LedgerEntryIdConverter : ValueConverter<LedgerEntryId, Guid>
{
    public LedgerEntryIdConverter() : base(id => id.Value, value => new LedgerEntryId(value))
    {
    }
}
