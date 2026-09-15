using MediatR;

namespace CrossLedgerWeb.Application.Auth;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<LoginResult>;
