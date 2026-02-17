using Microsoft.AspNetCore.Mvc;
using Workflow.Api.DTOs;
using Workflow.Api.Services;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _service;

    public RequestsController(IRequestService service)
    {
        _service = service;
    }

    // GET: api/requests
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (!TryGetUserContext(out var userId, out var role)) return Unauthorized("Missing X-User-Id or X-Role headers.");
        
        var requests = await _service.GetAllAsync(userId, role, status);
        return Ok(requests);
    }

    // GET: api/requests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetUserContext(out var userId, out var role)) return Unauthorized("Missing X-User-Id or X-Role headers.");

        var request = await _service.GetByIdAsync(id, userId, role);
        if (request == null) return NotFound();

        return Ok(request);
    }

    // POST: api/requests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        if (!TryGetUserContext(out var userId, out _)) return Unauthorized("Missing X-User-Id or X-Role headers.");

        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // POST: api/requests/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] DecisionDto dto)
    {
        if (!TryGetUserContext(out var userId, out var role)) return Unauthorized("Missing X-User-Id or X-Role headers.");
        if (role != "Manager") return Forbid();

        var success = await _service.ApproveAsync(id, userId, dto);
        if (!success) return BadRequest("Request cannot be approved (invalid status or ID).");

        return NoContent();
    }

    // POST: api/requests/{id}/reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] DecisionDto dto)
    {
        if (!TryGetUserContext(out var userId, out var role)) return Unauthorized("Missing X-User-Id or X-Role headers.");
        if (role != "Manager") return Forbid();

        try
        {
            var success = await _service.RejectAsync(id, userId, dto);
            if (!success) return BadRequest("Request cannot be rejected (invalid status or ID).");
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/requests/{id}/history
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _service.GetHistoryAsync(id);
        return Ok(history);
    }

    // Helper to simulate Auth until JWT is implemented
    private bool TryGetUserContext(out Guid userId, out string role)
    {
        userId = Guid.Empty;
        role = string.Empty;

        if (Request.Headers.TryGetValue("X-User-Id", out var idHeader) &&
            Request.Headers.TryGetValue("X-Role", out var roleHeader) &&
            Guid.TryParse(idHeader, out userId))
        {
            role = roleHeader.ToString();
            return true;
        }
        return false;
    }
}
