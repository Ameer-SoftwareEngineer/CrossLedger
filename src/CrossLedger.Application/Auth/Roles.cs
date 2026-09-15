namespace CrossLedger.Application.Auth;

/// <summary>The three roles from specification 6: Customer, Support and Admin
/// separation enforced per endpoint.</summary>
public static class Roles
{
    public const string Customer = "Customer";
    public const string Support = "Support";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Customer, Support, Admin];
}
