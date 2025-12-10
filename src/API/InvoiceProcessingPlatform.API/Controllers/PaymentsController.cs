using PaymentTracking.Application.Commands.CompletePayment;
using PaymentTracking.Application.Commands.SchedulePayment;
using PaymentTracking.Application.Queries;
using PaymentTracking.Application.Queries.GetPaymentById;
using PaymentTracking.Application.Queries.GetPaymentByInvoiceId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessingPlatform.API.Controllers;

/// <summary>
/// API controller for Payment tracking operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get a payment by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _sender.Send(new GetPaymentByIdQuery(id), cancellationToken);
        
        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Get payment by invoice ID.
    /// </summary>
    [HttpGet("by-invoice/{invoiceId:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByInvoiceId(Guid invoiceId, CancellationToken cancellationToken)
    {
        var payment = await _sender.Send(new GetPaymentByInvoiceIdQuery(invoiceId), cancellationToken);
        
        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Schedule a payment for an approved invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Schedule([FromBody] SchedulePaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new SchedulePaymentCommand(
            request.InvoiceId,
            request.VendorId,
            request.Amount,
            request.Currency,
            request.ScheduledDate,
            "system"); // TODO: Get from authenticated user

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Complete a payment.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompletePaymentCommand(id, request.TransactionReference), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok();
    }
}

// Request DTOs
public record SchedulePaymentRequest(
    Guid InvoiceId,
    Guid VendorId,
    decimal Amount,
    string Currency,
    DateTime ScheduledDate);

public record CompletePaymentRequest(string TransactionReference);
