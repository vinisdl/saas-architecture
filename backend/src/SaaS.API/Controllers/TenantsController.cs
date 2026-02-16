using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Tenants;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<CreateTenantResult>> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTenantCommand(request.Name, request.Slug), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.TenantId }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> List(CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new ListTenantsQuery(), cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _mediator.Send(new GetTenantByIdQuery(id), cancellationToken);
        if (tenant == null)
            return NotFound();
        return Ok(tenant);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new UpdateTenantCommand(id, request.Name, request.IsActive), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == "Tenant not found.")
        {
            return NotFound();
        }
    }
}

public record CreateTenantRequest(string Name, string Slug);

public record UpdateTenantRequest(string? Name, bool? IsActive);
