namespace CrossLedgerWeb.Domain.ValueObjects;

/// <summary>
/// An ISO 4217 currency code paired with its minor-unit scale (e.g. USD = 2 decimal
/// places, JPY = 0, KWD = 3). Only currencies in <see cref="KnownCurrencies"/> can be
/// constructed, so an unsupported code fails at the boundary rather than silently
/// defaulting to a scale that could corrupt ledger arithmetic.
/// </summary>
public readonly record struct Currency
{
    // ISO 4217 minor units. Everything not listed here is assumed to be the common
    // case (2 decimal places); the zero- and three-decimal sets are the real exceptions.
    private static readonly IReadOnlySet<string> ZeroDecimalCodes =
        new HashSet<string> { "JPY", "KRW", "VND", "CLP", "ISK" };

    private static readonly IReadOnlySet<string> ThreeDecimalCodes =
        new HashSet<string> { "KWD", "BHD", "OMR", "JOD", "TND", "IQD" };

    private static readonly IReadOnlySet<string> KnownCurrencies = new HashSet<string>(
        ZeroDecimalCodes
            .Concat(ThreeDecimalCodes)
            .Concat(new[]
            {
                "USD", "EUR", "GBP", "PKR", "INR", "AED", "SAR", "CNY", "AUD", "CAD",
                "CHF", "SGD", "HKD", "NZD", "ZAR", "TRY", "THB", "MXN", "BRL", "RUB",
                "NGN", "EGP", "QAR", "MYR",
            }));

    public string Code { get; }
    public int DecimalPlaces { get; }

    private Currency(string code, int decimalPlaces)
    {
        Code = code;
        DecimalPlaces = decimalPlaces;
    }

    public static Currency From(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();
        if (!KnownCurrencies.Contains(normalized))
            throw new ArgumentException($"Unknown or unsupported currency code '{code}'.", nameof(code));

        var decimalPlaces = ZeroDecimalCodes.Contains(normalized) ? 0
            : ThreeDecimalCodes.Contains(normalized) ? 3
            : 2;

        return new Currency(normalized, decimalPlaces);
    }

    public override string ToString() => Code;
}
