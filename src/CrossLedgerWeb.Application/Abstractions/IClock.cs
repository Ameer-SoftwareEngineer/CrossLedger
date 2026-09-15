namespace CrossLedgerWeb.Application.Abstractions;

/// <summary>Injected so handlers never call DateTimeOffset.UtcNow directly, keeping
/// "now" deterministic and controllable in tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
