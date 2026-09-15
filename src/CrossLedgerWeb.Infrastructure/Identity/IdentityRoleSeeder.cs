using CrossLedgerWeb.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace CrossLedgerWeb.Infrastructure.Identity;

/// <summary>Ensures Customer/Support/Admin (specification 6) exist before any user can
/// be assigned to them. Called once at startup from Program.cs, not on every request.</summary>
public static class IdentityRoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }
}
