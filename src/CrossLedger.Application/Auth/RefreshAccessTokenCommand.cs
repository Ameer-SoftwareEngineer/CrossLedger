using MediatR;

namespace CrossLedger.Application.Auth;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<LoginResult>;
