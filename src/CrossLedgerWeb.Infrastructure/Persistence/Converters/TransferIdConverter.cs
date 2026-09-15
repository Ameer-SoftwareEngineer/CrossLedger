using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class TransferIdConverter : ValueConverter<TransferId, Guid>
{
    public TransferIdConverter() : base(id => id.Value, value => new TransferId(value))
    {
    }
}
