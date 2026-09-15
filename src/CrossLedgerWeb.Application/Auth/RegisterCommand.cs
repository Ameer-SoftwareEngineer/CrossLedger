using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Auth;

public sealed record RegisterCommand(string Email, string Password) : IRequest<RegisterResult>;

public sealed record RegisterResult(UserId UserId, string Email);
