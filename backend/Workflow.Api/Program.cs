using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using Workflow.Api.Data;
using Workflow.Api.Models;
using Workflow.Api.Repositories;
using Workflow.Api.Services;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddControllers();

SqlMapper.AddTypeHandler(new StringEnumHandler<UserRole>());
SqlMapper.AddTypeHandler(new StringEnumHandler<RequestPriority>());
SqlMapper.AddTypeHandler(new StringEnumHandler<RequestStatus>());

builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestService, RequestService>();

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_key_for_dev_1234567890");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

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


app.UseCors("AllowAngularApp");
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/db-check", async (IConfiguration config) =>
{
    using var connection = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
    var time = await connection.QueryFirstAsync<DateTime>("SELECT NOW()");
    return Results.Ok(new { Status = "Connected", DatabaseTime = time });
});

app.Run();
