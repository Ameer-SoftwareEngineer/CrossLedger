using CrossLedger.Application.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

/// <summary>
/// The boundary around ASP.NET Core Identity: Application depends on this, never on
/// UserManager/SignInManager directly, so the password hashing scheme, lockout policy
/// and user store stay Infrastructure's concern (specification 6.1).
/// </summary>
public interface IIdentityService
{
    Task<RegistrationOutcome> RegisterAsync(string email, string password, CancellationToken cancellationToken);

    Task<CredentialValidationOutcome> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);

    Task<UserProfile?> GetProfileAsync(UserId userId, CancellationToken cancellationToken);
}
