using Dapper;
using Npgsql;

namespace Workflow.Api.Data;

public static class DbConfig
{
    public static void InitializeWithoutSeeding(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        
        CreateTables(connection);
    }
    public static void InitializeWithSeeding(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        
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
            var userSql = @"
                INSERT INTO Users (Username, PasswordHash, Role) VALUES 
                ('colaborador_comum', 'hash_senha_123', 'User'),
                ('ana_silva', 'senha_user_2', 'User'),
                ('joao_santos', 'senha_user_3', 'User'),
                ('gerente_sistema', 'hash_senha_456', 'Manager'),
                ('mariana_gerente', 'senha_manager_2', 'Manager');";
            connection.Execute(userSql);
        }

        var requestExists = connection.ExecuteScalar<bool>("SELECT EXISTS (SELECT 1 FROM Requests LIMIT 1)");
        if (!requestExists)
        {
            var requestSql = @"
                DO $$
                DECLARE
                    v_user_colab UUID;
                    v_user_ana UUID;
                    v_user_joao UUID;
                    v_manager UUID;
                    v_req_id UUID;
                BEGIN
                    SELECT Id INTO v_user_colab FROM Users WHERE Username = 'colaborador_comum';
                    SELECT Id INTO v_user_ana FROM Users WHERE Username = 'ana_silva';
                    SELECT Id INTO v_user_joao FROM Users WHERE Username = 'joao_santos';
                    SELECT Id INTO v_manager FROM Users WHERE Username = 'gerente_sistema';

                    -- 1. Impressora
                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Impressora com defeito', 'A impressora do 2º andar está atolando papel.', 'Hardware', 'Medium', 'Pending', v_user_colab)
                    RETURNING Id INTO v_req_id;
                    
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_colab, NOW(), 'Solicitação criada via seed.');

                    -- 2. VPN
                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt) 
                    VALUES ('Acesso VPN', 'Solicito acesso à VPN para trabalho remoto.', 'Acesso', 'High', 'Rejected', v_user_ana, NOW() - INTERVAL '1 day', NOW())
                    RETURNING Id INTO v_req_id;

                    -- History 1: Creation (User)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_ana, NOW() - INTERVAL '1 day', 'Solicitação criada via seed.');

                    -- History 2: Update (Manager)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, 'Pending', 'Rejected', v_manager, NOW(), 'Rejeitado pelo gerente.');

                    -- 3. Monitor
                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Monitor piscando', 'O monitor secundário apaga de vez em quando.', 'Hardware', 'Low', 'Pending', v_user_joao)
                    RETURNING Id INTO v_req_id;

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_joao, NOW(), 'Solicitação criada via seed.');

                    -- 4. ERP
                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Erro no sistema ERP', 'Erro 500 ao tentar gerar relatório mensal.', 'Software', 'High', 'Pending', v_user_colab)
                    RETURNING Id INTO v_req_id;

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_colab, NOW(), 'Solicitação criada via seed.');

                    -- 5. Docker
                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt) 
                    VALUES ('Instalação do Docker', 'Preciso do Docker instalado para o novo projeto.', 'Software', 'Medium', 'Approved', v_user_ana, NOW() - INTERVAL '2 days', NOW())
                    RETURNING Id INTO v_req_id;

                    -- History 1: Creation (User)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_ana, NOW() - INTERVAL '2 days', 'Solicitação criada via seed.');

                    -- History 2: Update (Manager)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, 'Pending', 'Approved', v_manager, NOW(), 'Aprovado pelo gerente.');
                END $$;";
            
            connection.Execute(requestSql);
        }
    }
}