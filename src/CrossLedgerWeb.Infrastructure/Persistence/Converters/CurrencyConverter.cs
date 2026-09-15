using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

/// <summary>Stores only the ISO 4217 code — DecimalPlaces is re-derived by
/// <see cref="Currency.From"/> on read, never persisted independently.</summary>
public sealed class CurrencyConverter : ValueConverter<Currency, string>
{
    public CurrencyConverter() : base(currency => currency.Code, code => Currency.From(code))
    {
    }
}
