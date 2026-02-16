using Dapper;
using Npgsql;
using Workflow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    DbConfig.InitializeWithSeeding(connectionString!);
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Program");
    logger.LogError(ex, "An error occurred while initializing the database.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/db-check", async (IConfiguration config) =>
{
    using var connection = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
    var time = await connection.QueryFirstAsync<DateTime>("SELECT NOW()");
    return Results.Ok(new { Status = "Connected", DatabaseTime = time });
});

app.Run();
