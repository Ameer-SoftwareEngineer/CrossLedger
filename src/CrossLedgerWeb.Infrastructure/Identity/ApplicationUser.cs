using Microsoft.AspNetCore.Identity;

namespace CrossLedgerWeb.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity's user record. Deliberately not the same type as
/// Domain's UserId/User concept - Identity's IdentityUser carries framework-specific
/// concerns (password hash, lockout counters, security stamp) that Domain has no
/// business knowing about. Its Id doubles as the UserId Guid used everywhere else.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>;
