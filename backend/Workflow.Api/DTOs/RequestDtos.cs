namespace Workflow.Api.DTOs;

// POST /requests
public record CreateRequestDto(
    string Title, 
    string Description, 
    string Category, 
    string Priority
);

// GET /requests e GET /requests/{id}
public record RequestResponseDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string Priority,
    string Status,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// POST /requests/{id}/approve e /reject
public record DecisionDto(
    string? Comment
);

// GET /requests/{id}/history
public record RequestHistoryDto(
    Guid Id,
    string FromStatus,
    string ToStatus,
    string ChangedByName,
    DateTime ChangedAt,
    string? Comment
);