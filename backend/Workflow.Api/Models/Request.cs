namespace Workflow.Api.Models;

public class Request
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public RequestPriority Priority { get; set; } = RequestPriority.Low;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}