using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class QuoteIdConverter : ValueConverter<QuoteId, Guid>
{
    public QuoteIdConverter() : base(id => id.Value, value => new QuoteId(value))
    {
    }
}
