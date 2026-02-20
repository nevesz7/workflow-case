using Dapper;
using Npgsql;
using Workflow.Api.DTOs;
using Workflow.Api.Models;

namespace Workflow.Api.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly string _connectionString;

    public RequestRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<RequestResponseDto>> GetRequestsAsync(Guid? userId, string? status)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT 
                Id, Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt
            FROM Requests
            WHERE (@UserId IS NULL OR UserId = @UserId)
              AND (@Status IS NULL OR Status = @Status)
            ORDER BY CreatedAt DESC";
        
        return await connection.QueryAsync<RequestResponseDto>(sql, new { UserId = userId, Status = status });
    }

    public async Task<RequestResponseDto?> GetByIdAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT 
                Id, Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt
            FROM Requests
            WHERE Id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<RequestResponseDto>(sql, new { Id = id });
    }

    public async Task CreateAsync(Request request)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var sql = @"
                INSERT INTO Requests (Id, Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt)
                VALUES (@Id, @Title, @Description, @Category, @Priority, @Status, @UserId, @CreatedAt, @UpdatedAt)";
            
            var parameters = new
            {
                request.Id,
                request.Title,
                request.Description,
                request.Category,
                Priority = request.Priority.ToString(),
                Status = request.Status.ToString(),
                request.UserId,
                request.CreatedAt,
                request.UpdatedAt
            };
            
            await connection.ExecuteAsync(sql, parameters, transaction);

            var sqlHistory = @"
                INSERT INTO RequestHistory (Id, RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                VALUES (gen_random_uuid(), @Id, NULL, @Status, @UserId, @CreatedAt, 'Solicitação criada pelo usuário.')";

            await connection.ExecuteAsync(sqlHistory, parameters, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid managerId, string? comment)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var currentStatus = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Status FROM Requests WHERE Id = @Id", new { Id = id }, transaction);

            if (currentStatus == null) return false;

            var updateSql = "UPDATE Requests SET Status = @Status, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            var rows = await connection.ExecuteAsync(updateSql, new { Id = id, Status = status }, transaction);

            if (rows > 0)
            {
                var historySql = @"
                    INSERT INTO RequestHistory (Id, RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (gen_random_uuid(), @RequestId, @FromStatus, @ToStatus, @ChangedBy, CURRENT_TIMESTAMP, @Comment)";
                
                await connection.ExecuteAsync(historySql, new {
                    RequestId = id,
                    FromStatus = currentStatus,
                    ToStatus = status,
                    ChangedBy = managerId,
                    Comment = comment
                }, transaction);

                await transaction.CommitAsync();
                return true;
            }

            return false;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<RequestHistoryDto>> GetHistoryAsync(Guid requestId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT 
                h.Id, h.FromStatus, h.ToStatus, u.Username as ChangedByName, h.ChangedAt, h.Comment
            FROM RequestHistory h
            JOIN Users u ON h.ChangedBy = u.Id
            WHERE h.RequestId = @RequestId
            ORDER BY h.ChangedAt DESC";
        
        return await connection.QueryAsync<RequestHistoryDto>(sql, new { RequestId = requestId });
    }
}