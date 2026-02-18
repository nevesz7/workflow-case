using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Api.DTOs;
using Workflow.Api.Services;

namespace Workflow.Api.Controllers;

[Authorize]
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
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        var requests = await _service.GetAllAsync(userId, role, status);
        return Ok(requests);
    }

    // GET: api/requests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        var request = await _service.GetByIdAsync(id, userId, role);
        if (request == null) return NotFound();

        return Ok(request);
    }

    // POST: api/requests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);

        try
        {
            var created = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException)
        {
            return BadRequest("Invalid data provided (e.g., invalid Priority).");
        }
    }

    // POST: api/requests/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] DecisionDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);
        if (!User.IsInRole("Manager")) return Forbid();

        var success = await _service.ApproveAsync(id, userId, dto);
        if (!success) return BadRequest("Request cannot be approved (invalid status or ID).");

        return NoContent();
    }

    // POST: api/requests/{id}/reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] DecisionDto dto)
    {
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);
        if (!User.IsInRole("Manager")) return Forbid();

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
        var userId = Guid.Parse(User.FindFirst("userId")!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        var history = await _service.GetHistoryAsync(id, userId, role);
        if (history == null) return NotFound();

        return Ok(history);
    }
}
