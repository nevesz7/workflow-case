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
                Role VARCHAR(20) NOT NULL DEFAULT 'User'
            );

            CREATE TABLE IF NOT EXISTS Requests (
                Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                Title VARCHAR(100) NOT NULL,
                Description TEXT,
                Category VARCHAR(50),
                Priority VARCHAR(20) DEFAULT 'Medium',
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
                -- Insert Users
                INSERT INTO Users (Username, PasswordHash, Role) VALUES 
                ('colaborador_comum', 'hash_senha_123', 'User'),
                ('ana_silva', 'senha_user_2', 'User'),
                ('joao_santos', 'senha_user_3', 'User'),
                ('gerente_sistema', 'hash_senha_456', 'Manager'),
                ('mariana_gerente', 'senha_manager_2', 'Manager');

                -- Insert Requests
                INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) VALUES 
                ('Impressora com defeito', 'A impressora do 2º andar está atolando papel.', 'Hardware', 'Medium', 'Pending', (SELECT Id FROM Users WHERE Username = 'colaborador_comum')),
                ('Acesso VPN', 'Solicito acesso à VPN para trabalho remoto.', 'Acesso', 'High', 'InProgress', (SELECT Id FROM Users WHERE Username = 'ana_silva')),
                ('Monitor piscando', 'O monitor secundário apaga de vez em quando.', 'Hardware', 'Low', 'Pending', (SELECT Id FROM Users WHERE Username = 'joao_santos')),
                ('Erro no sistema ERP', 'Erro 500 ao tentar gerar relatório mensal.', 'Software', 'High', 'Pending', (SELECT Id FROM Users WHERE Username = 'colaborador_comum')),
                ('Instalação do Docker', 'Preciso do Docker instalado para o novo projeto.', 'Software', 'Medium', 'Approved', (SELECT Id FROM Users WHERE Username = 'ana_silva'));";
            
            connection.Execute(seedSql);
        }
    }
}