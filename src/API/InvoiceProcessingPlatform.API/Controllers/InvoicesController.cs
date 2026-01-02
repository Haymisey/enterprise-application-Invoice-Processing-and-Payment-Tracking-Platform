using InvoiceManagement.Application.Commands.ApproveInvoice;
using InvoiceManagement.Application.Commands.CreateInvoice;
using InvoiceManagement.Application.Commands.SubmitInvoice;
using InvoiceManagement.Application.Queries;
using InvoiceManagement.Application.Queries.GetInvoiceById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessingPlatform.API.Controllers;

/// <summary>
/// API controller for Invoice management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public InvoicesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get an invoice by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        
        if (invoice is null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }

    /// <summary>
    /// Create a new invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(
            request.InvoiceNumber,
            request.VendorId,
            request.IssueDate,
            request.DueDate,
            "system", // TODO: Get from authenticated user
            request.Notes,
            request.LineItems.Select(li => new CreateInvoiceLineItemDto(
                li.Description, li.Quantity, li.UnitPrice)).ToList());

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Submit an invoice for approval.
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SubmitInvoiceCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok();
    }

    /// <summary>
    /// Approve an invoice for payment.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveInvoiceCommand(id, request.ApprovedBy), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok();
    }
}

// Request DTOs
public record CreateInvoiceRequest(
    string InvoiceNumber,
    Guid VendorId,
    DateTime IssueDate,
    DateTime DueDate,
    string? Notes,
    List<LineItemRequest> LineItems);

public record LineItemRequest(
    string Description,
    int Quantity,
    decimal UnitPrice);

public record ApproveInvoiceRequest(string ApprovedBy);
