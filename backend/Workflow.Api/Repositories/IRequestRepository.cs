using Workflow.Api.DTOs;
using Workflow.Api.Models;

namespace Workflow.Api.Repositories;

public interface IRequestRepository
{
    Task<IEnumerable<RequestResponseDto>> GetRequestsAsync(Guid? userId, string? status);
    Task<RequestResponseDto?> GetByIdAsync(Guid id);
    Task CreateAsync(Request request);
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid managerId, string? comment);
    Task<IEnumerable<RequestHistoryDto>> GetHistoryAsync(Guid requestId);
}