using Dapper;
using Npgsql;

namespace Workflow.Api.Data;

public static class DbConfig
{
    public static void InitializeWithoutSeeding(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        
        CreateTables(connection);
    }
    public static void InitializeWithSeeding(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        
        CreateTables(connection);
        SeedData(connection);
    }

    private static void CreateTables(NpgsqlConnection connection)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                Username VARCHAR(50) NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role VARCHAR(20) NOT NULL CHECK (Role IN ('User', 'Manager'))
            );

            CREATE TABLE IF NOT EXISTS Requests (
                Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                Title VARCHAR(100) NOT NULL,
                Description TEXT,
                Category VARCHAR(50),
                Priority INT DEFAULT 1,
                Status VARCHAR(20) DEFAULT 'Pending',
                UserId UUID REFERENCES Users(Id),
                CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS RequestHistory (
                Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                RequestId UUID REFERENCES Requests(Id) ON DELETE CASCADE,
                FromStatus VARCHAR(20),
                ToStatus VARCHAR(20),
                ChangedBy UUID REFERENCES Users(Id),
                ChangedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                Comment TEXT
            );";
        
        connection.Execute(sql);
    }

    private static void SeedData(NpgsqlConnection connection)
    {
        var userExists = connection.ExecuteScalar<bool>("SELECT EXISTS (SELECT 1 FROM Users LIMIT 1)");

        if (!userExists)
        {
            var seedSql = @"
                INSERT INTO Users (Username, PasswordHash, Role) VALUES 
                ('colaborador_comum', 'hash_senha_123', 'User'),
                ('gerente_sistema', 'hash_senha_456', 'Manager');";
            
            connection.Execute(seedSql);
        }
    }
}