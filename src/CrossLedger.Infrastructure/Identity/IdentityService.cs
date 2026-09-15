using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Auth;
using CrossLedger.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace CrossLedger.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<RegistrationOutcome> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return RegistrationOutcome.Failure(result.Errors.Select(e => e.Description).ToList());

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        return RegistrationOutcome.Success(new UserId(user.Id));
    }

    public async Task<CredentialValidationOutcome> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return CredentialValidationOutcome.Failed;

        // lockoutOnFailure: true delegates the increment-on-failure/reset-on-success
        // bookkeeping to Identity itself rather than this service re-implementing it.
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return CredentialValidationOutcome.LockedOut;

        return result.Succeeded
            ? CredentialValidationOutcome.Success(new UserId(user.Id))
            : CredentialValidationOutcome.Failed;
    }

    public async Task<UserProfile?> GetProfileAsync(UserId userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfile(userId, user.Email!, roles.ToList());
    }
}
