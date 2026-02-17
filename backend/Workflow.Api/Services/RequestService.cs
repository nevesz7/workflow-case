using Workflow.Api.DTOs;
using Workflow.Api.Models;
using Workflow.Api.Repositories;

namespace Workflow.Api.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _repository;

    public RequestService(IRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RequestResponseDto>> GetAllAsync(Guid userId, string role, string? status)
    {
        // Manager vê tudo, User vê apenas o dele
        var filterUserId = role == "Manager" ? null : (Guid?)userId;
        return await _repository.GetRequestsAsync(filterUserId, status);
    }

    public async Task<RequestResponseDto?> GetByIdAsync(Guid id, Guid userId, string role)
    {
        var request = await _repository.GetByIdAsync(id);
        if (request == null) return null;

        // Segurança: Se não for manager e não for o dono, bloqueia
        if (role != "Manager" && request.UserId != userId) return null;

        return request;
    }

    public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, Guid userId)
    {
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = Enum.Parse<RequestPriority>(dto.Priority, true),
            Status = RequestStatus.Pending, // Workflow: Sempre inicia como Pending
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(request);
        return (await GetByIdAsync(request.Id, userId, "User"))!;
    }

    public async Task<bool> ApproveAsync(Guid id, Guid managerId, DecisionDto dto)
    {
        var request = await _repository.GetByIdAsync(id);
        
        // Regra: Apenas Pending pode ser aprovado
        if (request == null || request.Status != "Pending") return false;

        return await _repository.UpdateStatusAsync(id, "Approved", managerId, dto.Comment);
    }

    public async Task<bool> RejectAsync(Guid id, Guid managerId, DecisionDto dto)
    {
        // Regra de Negócio: Comentário obrigatório para Reprovar
        if (string.IsNullOrWhiteSpace(dto.Comment))
            throw new ArgumentException("O comentário é obrigatório para rejeitar uma solicitação.");

        var request = await _repository.GetByIdAsync(id);
        if (request == null || request.Status != "Pending") return false;

        return await _repository.UpdateStatusAsync(id, "Rejected", managerId, dto.Comment);
    }

    public async Task<IEnumerable<RequestHistoryDto>> GetHistoryAsync(Guid requestId)
    {
        return await _repository.GetHistoryAsync(requestId);
    }
}