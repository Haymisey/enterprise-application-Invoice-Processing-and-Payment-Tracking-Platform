using VendorManagement.Application.Commands.ActivateVendor;
using VendorManagement.Application.Commands.RegisterVendor;
using VendorManagement.Application.Queries;
using VendorManagement.Application.Queries.GetVendorById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessingPlatform.API.Controllers;

/// <summary>
/// API controller for Vendor management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly ISender _sender;

    public VendorsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get a vendor by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vendor = await _sender.Send(new GetVendorByIdQuery(id), cancellationToken);
        
        if (vendor is null)
        {
            return NotFound();
        }

        return Ok(vendor);
    }

    /// <summary>
    /// Register a new vendor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterVendorRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterVendorCommand(
            request.Name,
            request.TaxId,
            request.Email,
            request.Phone,
            request.ContactPerson,
            request.PaymentTermDays,
            "system"); // TODO: Get from authenticated user

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Activate a vendor.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateVendorRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateVendorCommand(id, request.ActivatedBy), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Description });
        }

        return Ok();
    }
}

// Request DTOs
public record RegisterVendorRequest(
    string Name,
    string TaxId,
    string Email,
    string Phone,
    string? ContactPerson,
    int PaymentTermDays = 30);

public record ActivateVendorRequest(string ActivatedBy);
