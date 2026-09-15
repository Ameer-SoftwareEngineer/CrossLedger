namespace CrossLedgerWeb.Shared.Auth;

public sealed record RegisterRequest(string Email, string Password);

public sealed record RegisterResponse(Guid UserId, string Email);
