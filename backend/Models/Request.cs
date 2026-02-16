namespace Workflow.Api.Models;

public class Request
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int Priority { get; set; } = "Baixa"; // 1: Baixa, 2: Média, 3: Alta
    public string Status { get; set; } = "Pending";
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}