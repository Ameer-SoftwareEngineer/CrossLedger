using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class RefreshTokenIdConverter : ValueConverter<RefreshTokenId, Guid>
{
    public RefreshTokenIdConverter() : base(id => id.Value, value => new RefreshTokenId(value))
    {
    }
}
