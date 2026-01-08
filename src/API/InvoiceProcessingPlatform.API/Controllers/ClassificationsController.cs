using MediatR;
using Microsoft.AspNetCore.Mvc;
using AIClassification.Application.Commands.ClassifyInvoice;
using AIClassification.Application.Queries.GetClassification;

namespace InvoiceProcessingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassificationsController : ControllerBase
{
    private readonly ISender _sender;

    public ClassificationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Upload an invoice image for AI classification and extraction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClassifyInvoice([FromBody] ClassifyInvoiceRequest request)
    {
        var command = new ClassifyInvoiceCommand(request.ImageUrl);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return AcceptedAtAction(nameof(GetClassification), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Get the status and results of an AI classification.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassification(Guid id)
    {
        var query = new GetClassificationQuery(id);
        var result = await _sender.Send(query);

        if (result is null) return NotFound();

        return Ok(result);
    }
}

public record ClassifyInvoiceRequest(string ImageUrl);
