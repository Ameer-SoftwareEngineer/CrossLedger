using CrossLedger.Api.Security;
using CrossLedger.Application.Auth;
using CrossLedger.Application.Transfers;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Shared.Transfers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrossLedger.Api.Controllers;

[ApiController]
[Route("api/v1/transfers")]
public sealed class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Every mutating endpoint requires an Idempotency-Key header
    /// (specification 2.3) - a repeated key replays the stored response instead of
    /// posting the transfer again. RequireStepUp only actually challenges the caller
    /// once SourceAmount clears Limits:StepUpAbove (specification 6.2's worked example,
    /// 6.3) - everyday transfers below the threshold pass straight through.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [RequireStepUp(Operation = StepUpOperation.HighValueTransfer, ThresholdSetting = "Limits:StepUpAbove")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TransferResponse>> Create(
        CreateTransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransferCommand(
            new QuoteId(request.QuoteId),
            new WalletId(request.SourceWalletId),
            new WalletId(request.TargetWalletId),
            request.SourceAmount,
            idempotencyKey);

        var result = await _mediator.Send(command, cancellationToken);

        var response = new TransferResponse(
            result.TransferId.Value,
            result.SourceAmount.Amount,
            result.SourceAmount.Currency.Code,
            result.TargetAmount.Amount,
            result.TargetAmount.Currency.Code,
            result.PostedAt);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
