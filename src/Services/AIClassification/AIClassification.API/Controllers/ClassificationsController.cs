using MediatR;
using Microsoft.AspNetCore.Mvc;
using AIClassification.Application.Commands.ClassifyInvoice;
using AIClassification.Application.Queries.GetClassification;

namespace AIClassification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassificationsController : ControllerBase
{
    private readonly ISender _sender;

    public ClassificationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> ClassifyInvoice([FromBody] ClassifyInvoiceRequest request)
    {
        var command = new ClassifyInvoiceCommand(request.ImageUrl);
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return AcceptedAtAction(nameof(GetClassification), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClassification(Guid id)
    {
        var query = new GetClassificationQuery(id);
        var result = await _sender.Send(query);

        if (result is null) return NotFound();

        return Ok(result);
    }
}

public record ClassifyInvoiceRequest(string ImageUrl);