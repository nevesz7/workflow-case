using Dapper;
using Npgsql;
using BCrypt.Net;

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
                ('User1', @HashedPassword1, 'User'),
                ('User2', @HashedPassword2, 'User'),
                ('User3', @HashedPassword3, 'User'),
                ('Manager1', @AdminPassword1, 'Manager'),
                ('Manager2', @AdminPassword2, 'Manager');";
            connection.Execute(userSql, new {
                HashedPassword1 = BCrypt.Net.BCrypt.HashPassword("HashedPassword1word"),
                HashedPassword2 = BCrypt.Net.BCrypt.HashPassword("HashedPassword2test"),
                HashedPassword3 = BCrypt.Net.BCrypt.HashPassword("HashedPassword3user"),
                AdminPassword1 = BCrypt.Net.BCrypt.HashPassword("AdminPassword1deal"),
                AdminPassword2 = BCrypt.Net.BCrypt.HashPassword("AdminPassword2maker")
            });
        }

        var requestExists = connection.ExecuteScalar<bool>("SELECT EXISTS (SELECT 1 FROM Requests LIMIT 1)");
        if (!requestExists)
        {
            var requestSql = @"
                DO $$
                DECLARE
                    v_user_1 UUID;
                    v_user_2 UUID;
                    v_user_3 UUID;
                    v_manager_1 UUID;
                    v_manager_2 UUID;
                    v_req_id UUID;
                BEGIN
                    SELECT Id INTO v_user_1 FROM Users WHERE Username = 'User1';
                    SELECT Id INTO v_user_2 FROM Users WHERE Username = 'User2';
                    SELECT Id INTO v_user_3 FROM Users WHERE Username = 'User3';
                    SELECT Id INTO v_manager_1 FROM Users WHERE Username = 'Manager1';
                    SELECT Id INTO v_manager_2 FROM Users WHERE Username = 'Manager2';

                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Compras 1 - User 1', 'Description Medium Compras 1', 'Compras', 'Medium', 'Pending', v_user_1)
                    RETURNING Id INTO v_req_id;
                    
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_1, NOW(), 'Solicitação criada via seed.');

                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt) 
                    VALUES ('TI 1 - User 2', 'Description High TI 1', 'TI', 'High', 'Rejected', v_user_2, NOW() - INTERVAL '1 day', NOW())
                    RETURNING Id INTO v_req_id;

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_2, NOW() - INTERVAL '1 day', 'Solicitação criada via seed.');

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, 'Pending', 'Rejected', v_manager_2, NOW(), 'Rejected - Manager 2');

                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Reembolso 1 - User 3', 'Description Low Reembolso 1', 'Reembolso', 'Low', 'Pending', v_user_3)
                    RETURNING Id INTO v_req_id;

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_3, NOW(), 'Solicitação criada via seed.');

                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId) 
                    VALUES ('Outros 1 - User 1', 'Description High Outros 1', 'Outros', 'High', 'Pending', v_user_1)
                    RETURNING Id INTO v_req_id;

                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_1, NOW(), 'Solicitação criada via seed.');

                    INSERT INTO Requests (Title, Description, Category, Priority, Status, UserId, CreatedAt, UpdatedAt) 
                    VALUES ('Outros 2 - User 2', 'Description Medium Outros 2', 'Outros', 'Medium', 'Approved', v_user_2, NOW() - INTERVAL '2 days', NOW())
                    RETURNING Id INTO v_req_id;

                    -- History 1: Creation (User)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, NULL, 'Pending', v_user_2, NOW() - INTERVAL '2 days', 'Solicitação criada via seed.');

                    -- History 2: Update (Manager)
                    INSERT INTO RequestHistory (RequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Comment)
                    VALUES (v_req_id, 'Pending', 'Approved', v_manager_1, NOW(), 'Approved - Manager 1');
                END $$;";
            
            connection.Execute(requestSql);
        }
    }
}