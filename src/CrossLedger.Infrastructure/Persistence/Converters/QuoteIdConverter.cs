using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class QuoteIdConverter : ValueConverter<QuoteId, Guid>
{
    public QuoteIdConverter() : base(id => id.Value, value => new QuoteId(value))
    {
    }
}
