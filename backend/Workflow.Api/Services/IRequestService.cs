using Workflow.Api.DTOs;

namespace Workflow.Api.Services;

public interface IRequestService
{
    Task<IEnumerable<RequestResponseDto>> GetAllAsync(Guid userId, string role, string? status);
    Task<RequestResponseDto?> GetByIdAsync(Guid id, Guid userId, string role);
    Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, Guid userId);
    Task<bool> ApproveAsync(Guid id, Guid managerId, DecisionDto dto);
    Task<bool> RejectAsync(Guid id, Guid managerId, DecisionDto dto);
    Task<IEnumerable<RequestHistoryDto>> GetHistoryAsync(Guid requestId);
}